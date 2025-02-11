// include CUDA
#include <cuda_runtime.h>

// includes, project
#include <helper_cuda.h>
#include <helper_functions.h>

#define USE_ZEROCOPYMEMORY	0	// 0: use normal memory reserved by cudaHostAlloc(Default), 1: use zero copy memory reserved by cudaHostAlloc(Mapped).

__global__	void
blocksum( WORD *src, DWORD* dst, unsigned int n, DWORD *result )
{
	unsigned int tid = threadIdx.x;
	unsigned int idx = blockIdx.x * blockDim.x + threadIdx.x;

	if( idx >= n )	return;

	dst[ idx ] = src[ idx ];		// first 
	__syncthreads();

	for( unsigned int stride = 1; stride < blockDim.x; stride *= 2 )
	{
		if( tid % ( 2 * stride ) == 0 )
		{
			dst[ idx ] += dst[ idx + stride ];
		}
		__syncthreads();
	}

	if( tid == 0 )
	{
		result[ blockIdx.x ] = dst[ blockIdx.x * blockDim.x ];
	}
}

// calculate average center point by GPU
extern "C"
double calc_average_gpu( const void* buf, long rowbytes, long width, long height )
{
	WORD* g_src;
#if USE_ZEROCOPYMEMORY
	checkCudaErrors( cudaHostGetDevicePointer( (void**)&g_src, (void*)buf, 0 ) );		// get mapped pointer
#else
	long	framebytes = rowbytes * height;
	checkCudaErrors( cudaMalloc( (void**)&g_src, framebytes ) );						// allocate GPU memory
	checkCudaErrors( cudaMemcpy( g_src, buf, framebytes, cudaMemcpyHostToDevice ) );	// copy CPU memory to GPU
#endif

	// breakdown into threads and blocks.
	DWORD	threadNum	= 256;
	DWORD	blockNum;

	DWORD	matrixNum = width * height;
	if( matrixNum % threadNum )
	{
		matrixNum += ( threadNum - matrixNum % threadNum );
	}

	blockNum = matrixNum / threadNum;

	DWORD*	g_dst;
	checkCudaErrors( cudaMalloc( (void**)&g_dst, sizeof(DWORD) * matrixNum ) );
	checkCudaErrors( cudaMemset( g_dst, 0, sizeof(DWORD) * matrixNum ) );

	DWORD*	g_blockSum;
	checkCudaErrors( cudaMalloc( (void**)&g_blockSum, sizeof(DWORD) * blockNum ) );

	// Run Kernel
	dim3	threads( threadNum );									// thread number
	dim3	grid( blockNum );										// block number

	blocksum<<< grid, threads >>>( g_src, g_dst, (unsigned int)matrixNum, g_blockSum );		// execute in all threads

	// Copy result from GPU to CPU
	DWORD* h_blockSum = (DWORD*)malloc( sizeof(DWORD) * blockNum );
	checkCudaErrors( cudaMemcpy( h_blockSum, g_blockSum, sizeof(DWORD) * blockNum, cudaMemcpyDeviceToHost ) );

	double	total = 0;

	int	j;
	for( j=0; j<(long)blockNum; j++ )
	{
		total += h_blockSum[j];
	}

	checkCudaErrors( cudaFree( g_dst		) );
	checkCudaErrors( cudaFree( g_blockSum	) );

#if USE_ZEROCOPYMEMORY
	// nothing to do
#else
	checkCudaErrors( cudaFree( g_src		) );					// release GPU memory
#endif

	return total / width / height;
}

extern "C"
BOOL allocBuffer( void** buf, long bufsize )
{
#if USE_ZEROCOPYMEMORY
	// Setup Device
	long dev = 0;
	checkCudaErrors( cudaSetDevice( dev ) );

	// Get Device Property
	cudaDeviceProp	deviceProp;
	checkCudaErrors( cudaGetDeviceProperties( &deviceProp, dev ) );

	// Check Zero Copy Memory Supported
	if( ! deviceProp.canMapHostMemory )
	{
		printf( "Device %d does not support mapping CPU host memory.\n", dev );
		checkCudaErrors( cudaDeviceReset() );
		return FALSE;
	}

	checkCudaErrors( cudaHostAlloc( buf, bufsize, cudaHostAllocMapped ) );			// allocate CPU memory with mapping
#else
	checkCudaErrors( cudaHostAlloc( buf, bufsize, cudaHostAllocDefault ) );			// allocate CPU memory without mapping
#endif
	memset( *buf, 0, bufsize );

	return TRUE;
}

extern "C"
void releaseBuffer( void* buf )
{
	checkCudaErrors( cudaFreeHost( buf ) );											// release host memory
}