#crop window
xmin=40
xmax=120
ymin=20
ymax=100

xminref=40
xmaxref=120
yminref=20
ymaxref=100
    
def getWidth(img):
    xproj=np.sum(img,0)
    xproj=xproj/(1.0*np.sum(xproj))
    yproj=np.sum(img,1)
    yproj=yproj/(1.0*np.sum(yproj))
    meanx=np.sum(range(len(xproj))*xproj)
    meany=np.sum(range(len(yproj))*yproj)
    stdx=np.sqrt(np.sum(xproj*(range(len(xproj))-meanx)**2))
    stdy=np.sqrt(np.sum(yproj*(range(len(yproj))-meany)**2))
    #plt.figure()
    #plt.plot(xproj)
    return [meanx, meany, stdx, stdy]

def getWidthGaussianFit(img):
    xproj=np.sum(img,0)
    xproj=xproj/(1.0*np.sum(xproj))
    yproj=np.sum(img,1)
    yproj=yproj/(1.0*np.sum(yproj))
    return gaussianFit(np.array(range(len(xproj))),xproj)
    #return [meanx, meany, stdx, stdy]


def getImages(fileNo):
    archive=zipfile.ZipFile(dirPath + "\\" + fileNameString + "_" + str("%03d" % fileNo) +".zip",'r')
    files=archive.namelist()
    #files.sort(key=natural_keys)
    files.sort(key=lambda var:[int(x) if x.isdigit() else x for x in re.findall(r'[^0-9]|[0-9]+', var)])
    
    imgs=[]
    for f in files:
            if f[-3:]=='tif' and f[0]=='C':
                with archive.open(f) as filename:
                    imgs.append(np.array(Image.open(filename),dtype=float))
    return imgs[0:]


def getRImages(fileNo):
    archive=zipfile.ZipFile(dirPath + "\\" + fileNameString + "_" + str("%03d" % fileNo) +".zip",'r')
    files=archive.namelist()
    #files.sort(key=natural_keys)
    files.sort(key=lambda var:[int(x) if x.isdigit() else x for x in re.findall(r'[^0-9]|[0-9]+', var)])
    
    imgs=[]
    for f in files:
            if f[-3:]=='tif' and f[0]=='R' :
                with archive.open(f) as filename:
                    imgs.append(np.array(Image.open(filename),dtype=float))
    return imgs[0:]


def PlotListOfImages(imagelst):
    plt.figure(figsize=(20,10))
    for i in np.arange(len(imagelst)):
        plt.subplot(1,len(imagelst), i+1)
        plt.imshow(imagelst[i])

def PlotListOfProjectedImages(imagelst, ax):
    plt.figure(figsize=(20,5))
    lst = []
    for i in np.arange(len(imagelst)):
        plt.subplot(1,len(imagelst), i+1)
        plt.plot(np.sum(imagelst[i],axis=ax))
        lst.append(np.sum(imagelst[i],axis=ax))
        
    return lst

        
def ShowImages(listvals, navgBG, navg, filenumBGend, filenumend, project):
    
    filenumstart=filenumBGend-len(listvals)+1
    
    bgimglst=[]
    bgrefimglst=[]

    for fn in range(filenumstart,filenumBGend+1):
        c=getImages(fn)
        bgrefimg=[]
        bgimg=[]
        for i in range(navgBG):
            bgrefimg.append(c[2*i])
            bgimg.append(c[2*i+1])
        bgrefimglst.append(np.mean(bgrefimg,axis=0))
        bgimglst.append(np.mean(bgimg,axis=0))
    
    
    filenumstart=filenumend-len(listvals)+1
    
    imglst=[]
    refimglst=[]
    cnt=0
    for fn in range(filenumstart,filenumend+1):
        c=getImages(fn)
        refimgs=[]
        imgs=[]
        for i in range(navg):
            refimgs.append(c[2*i])
            imgs.append(c[2*i+1])
        refimglst.append(np.mean(refimgs,axis=0)-bgrefimglst[cnt])
        imglst.append(np.mean(imgs,axis=0)-bgimglst[cnt])
        cnt=cnt+1
    if project==True:
        lst = PlotListOfProjectedImages(imglst, 1)
        return lst
    else:
        #PlotListOfImages(bgimglst)
        PlotListOfImages(imglst)
        PlotListOfImages(refimglst)
        return [imglst, refimglst]
        #PlotListOfImages(refimglst)
        
def ShowImagesNoRef(listvals, navgBG,navg, filenumBGend, filenumend, project):
    
    filenumstart=filenumBGend-len(listvals)+1
    
    bgimglst=[]
    

    for fn in range(filenumstart,filenumBGend+1):
        c=getImages(fn)
        
        bgimg=[]
        for i in range(navgBG):
            
            bgimg.append(c[i])
        
        bgimglst.append(np.mean(bgimg,axis=0))
    
    
    filenumstart=filenumend-len(listvals)+1
    
    imglst=[]
    
    cnt=0
    for fn in range(filenumstart,filenumend+1):
        c=getImages(fn)
        
        imgs=[]
        for i in range(navg):
            
            imgs.append(c[i])
        
        imglst.append(np.mean(imgs,axis=0)-bgimglst[cnt])
        cnt=cnt+1
    if project==True:
        lst = PlotListOfProjectedImages(imglst, 1)
        return lst
    else:
        
        PlotListOfImages(imglst)
        
    return imglst

def ShowImagesTweezerChamber(listvals, navgBG, navg, filenumBGend, filenumend, project):
    
    filenumstart=filenumBGend-len(listvals)+1
    
    bgimglst=[]
    bgrefimglst=[]

    for fn in range(filenumstart,filenumBGend+1):
        c=getImages(fn)
        r=getRImages(fn)
        bgrefimg=[]
        bgimg=[]
        for i in range(navgBG):
            bgrefimg.append(c[i])
            bgimg.append(r[i])
        bgrefimglst.append(np.mean(bgrefimg,axis=0))
        bgimglst.append(np.mean(bgimg,axis=0))
    
    
    filenumstart=filenumend-len(listvals)+1
    
    imglst=[]
    refimglst=[]
    cnt=0
    for fn in range(filenumstart,filenumend+1):
        c=getImages(fn)
        r=getRImages(fn)
        refimgs=[]
        imgs=[]
        for i in range(navg):
            refimgs.append(c[i])
            imgs.append(r[i])
        refimglst.append(np.mean(refimgs,axis=0)-bgrefimglst[cnt])
        imglst.append(np.mean(imgs,axis=0)-bgimglst[cnt])
        cnt=cnt+1
    if project==True:
        lst = PlotListOfProjectedImages(imglst, 1)
        return lst
    else:
        #PlotListOfImages(bgimglst)
        PlotListOfImages(imglst)
        PlotListOfImages(refimglst)
        return [imglst, refimglst]
        #PlotListOfImages(refimglst)

def GetSignalToNoise(listvals, navg, filenumBGend, filenumend):
    filenumstart=filenumBGend-len(listvals)+1
    
    bgimgsum=[]
    bgrefimgsum=[]

    for fn in range(filenumstart,filenumBGend+1):
        c=getImages(fn)
        bgrefimg=[]
        bgimg=[]
        for i in range(navg):
            bgrefimg.append(c[2*i])
            bgimg.append(c[2*i+1])
        bgrefimgsum.append(np.sum(np.mean(bgrefimg,axis=0)))
        bgimgsum.append(np.sum(np.mean(bgrefimg,axis=0)))
    filenumstart=filenumend-len(listvals)+1
    
    imglst=[]
    refimglst=[]
    snrlst=[]
    cnt=0
    for fn in range(filenumstart,filenumend+1):
        c=getImages(fn)
        refimgs=[]
        imgs=[]
        for i in range(navg):
            refimgs.append(c[2*i])
            imgs.append(c[2*i+1])
        norm=np.sum(np.mean(refimgs,axis=0))-bgrefimgsum[cnt]
        snrlst.append((np.sum(np.mean(imgs,axis=0))/norm)/bgimgsum[cnt])
    plt.figure()
    plt.plot(listvals,snrlst)
    
    return snrlst
        
    
        
def MOTbasicscan(listvals, navg, filenumBG, filenumend, imagefigure):
    
    #background
    times=[1]
    
    
    filenumstart=filenumBG-len(times)+1
    
    sx=128
    sy=168
    
    
    img=[]
    refimg=[]
    
    for fn in range(filenumstart,filenumBG+1):
        c=getImages(fn)
        for i in range(navg):
            #refimg.append(c[2*i])
            img.append(c[i])
    
    
    meannormlst=[]
    stdnormlst=[]
    meanlst=[]
    stdlst=[]
    meanreflst=[]
    stdreflst=[]
    
    for i in range(len(times)):
        totimg=[]
        totrefimg=[]
        for j in range(0,navg):
            totimg.append(np.sum(img[i*navg+j][xminref:xmaxref,yminref:ymaxref]))
            #totrefimg.append(np.sum(refimg[i*navg+j][xminref:xmaxref,yminref:ymaxref]))
        #totimgnorm=(np.array(totimg))/(np.array(totrefimg))
       # meannormlst.append(np.mean(totimgnorm))
        meanlst.append(np.mean(totimg))
        #meanreflst.append(np.mean(totrefimg))
        #stdnormlst.append(np.std(totimgnorm))
        stdlst.append(np.std(totimg))
        #stdreflst.append(np.std(totrefimg))
    
    bg=meanlst[0]
    #bgref=meanreflst[0]
    
    
    
    filenumstart=filenumend-len(listvals)+1
    
    sx=128
    sy=168
    
    img=[]
    refimg=[]
    
    for fn in range(filenumstart,filenumend+1):
        c=getImages(fn)
        for i in range(navg):
            #refimg.append(c[2*i])
            img.append(c[i])
    
    
    meannormlst=[]
    stdnormlst=[]
    meanlst=[]
    stdlst=[]
    meanreflst=[]
    stdreflst=[]
    
    for i in range(len(listvals)):
        totimg=[]
        totrefimg=[]
        for j in range(0, navg):
            totimg.append(np.sum(img[i*navg+j][xminref:xmaxref,yminref:ymaxref]))
            #totrefimg.append(np.sum(refimg[i*navg+j][xminref:xmaxref,yminref:ymaxref]))
        #totimgnorm=(np.array(totimg)-bg)/(np.array(totrefimg)-bgref)
        #meannormlst.append(np.mean(totimgnorm))
        meanlst.append(np.mean(totimg))
        #meanreflst.append(np.mean(totrefimg))
        #stdnormlst.append(np.std(totimgnorm))
        stdlst.append(np.std(totimg))
        #stdreflst.append(np.std(totrefimg))
    
    
    plt.figure()
    plt.errorbar(np.array(listvals[0:]),meanlst[0:],stdlst[0:],fmt='o')
    #plt.figure()
    #plt.plot((np.array(totimg[1:])-bg)/(np.array(totrefimg[1:])-bgref),'o')
    
    if imagefigure == True:
        plt.figure()
        plt.imshow(img[1][xminref:xmaxref,yminref:ymaxref])
        
        
def ScanWithRefImageSingleBG(listvals, navg, filenumBG, filenumend, crop, imagefigure):
    #background
    times=[1]
    
    filenumstart=filenumBG-len(times)+1

    sx=128
    sy=168

    bgimg=[]
    bgrefimg=[]

    for fn in range(filenumstart,filenumBG+1):
        c=getImages(fn)
        for i in range(navg):
            if crop==True:
                bgrefimg.append(c[2*i][xminref:xmaxref,yminref:ymaxref])
                bgimg.append(c[2*i+1][xmin:xmax,ymin:ymax])
            else:
                bgrefimg.append(c[2*i])
                bgimg.append(c[2*i+1])


    meannormlst=[]
    stdnormlst=[]
    meanlst=[]
    stdlst=[]
    meanreflst=[]
    stdreflst=[]

    for i in range(len(times)):
        totimg=[]
        totrefimg=[]
        for j in range(1,navg):
            totimg.append(np.sum(bgimg[i*navg+j]))
            totrefimg.append(np.sum(bgrefimg[i*navg+j]))
        totimgnorm=(np.array(totimg))/(np.array(totrefimg))
        meannormlst.append(np.mean(totimgnorm))
        meanlst.append(np.mean(totimg))
        meanreflst.append(np.mean(totrefimg))
        stdnormlst.append(np.std(totimgnorm))
        stdlst.append(np.std(totimg))
        stdreflst.append(np.std(totrefimg))

    bg=meanlst[0]
    bgref=meanreflst[0]

    
    
    filenumstart=filenumend-len(listvals)+1

    sx=128
    sy=168

    img=[]
    refimg=[]

    for fn in range(filenumstart,filenumend+1):
        c=getImages(fn)
        for i in range(navg):
            if crop==True:
                refimg.append(c[2*i][xminref:xmaxref,yminref:ymaxref])
                img.append(c[2*i+1][xminref:xmaxref,yminref:ymaxref])
            else:
                refimg.append(c[2*i])
                img.append(c[2*i+1])


    meannormlst=[]
    stdnormlst=[]
    meanlst=[]
    stdlst=[]
    meanreflst=[]
    stdreflst=[]

    for i in range(len(listvals)):
        totimg=[]
        totrefimg=[]
        for j in range(1, navg):
            totimg.append(np.sum(img[i*navg+j]))
            totrefimg.append(np.sum(refimg[i*navg+j]))
        totimgnorm=(np.array(totimg)-bg)/(np.array(totrefimg)-bgref)
        meannormlst.append(np.mean(totimgnorm))
        meanlst.append(np.mean(totimg))
        meanreflst.append(np.mean(totrefimg))
        stdnormlst.append(np.std(totimgnorm))
        stdlst.append(np.std(totimg))
        stdreflst.append(np.std(totrefimg))

    plt.figure()
    plt.errorbar(np.array(listvals[:]),meannormlst[:],stdnormlst[:],fmt='o')
    if imagefigure == True:
        plt.figure()
        plt.imshow(img[1])
        plt.figure()
        plt.imshow(refimg[1])
        
    molasses = meannormlst
    molassesstd = stdnormlst
    return molasses, molassesstd

def ScanWithRefImageMultipleBG(listvals, navgBG, navg, filenumBG, filenumend):
    #background
    
    filenumstart=filenumBG-len(listvals)+1

    sx=128
    sy=168

    #crop window
    xmin=40
    xmax=95
    ymin=40
    ymax=95

    xminref=40
    xmaxref=100
    yminref=40
    ymaxref=100

    bgimg=[]
    bgrefimg=[]

    for fn in range(filenumstart,filenumBG+1):
        c=getImages(fn)
        for i in range(navgBG):
            bgrefimg.append(c[2*i])
            bgimg.append(c[2*i+1])


    meannormlst=[]
    stdnormlst=[]
    meanlst=[]
    stdlst=[]
    meanreflst=[]
    stdreflst=[]

    for i in range(len(listvals)):
        totimg=[]
        totrefimg=[]
        for j in range(1,navg):
            totimg.append(np.sum(bgimg[i*navg+j]))
            totrefimg.append(np.sum(bgrefimg[i*navg+j]))
        totimgnorm=(np.array(totimg))/(np.array(totrefimg))
        meannormlst.append(np.mean(totimgnorm))
        meanlst.append(np.mean(totimg))
        meanreflst.append(np.mean(totrefimg))
        stdnormlst.append(np.std(totimgnorm))
        stdlst.append(np.std(totimg))
        stdreflst.append(np.std(totrefimg))

    bgmeanlst = meanlst
    bgmeanreflst = meanreflst



    filenumstart=filenumend-len(listvals)+1

    sx=128
    sy=168

    img=[]
    refimg=[]

    for fn in range(filenumstart,filenumend+1):
        c=getImages(fn)
        for i in range(navg):
            refimg.append(c[2*i])
            img.append(c[2*i+1])


    meannormlst=[]
    stdnormlst=[]
    meanlst=[]
    stdlst=[]
    meanreflst=[]
    stdreflst=[]

    for i in range(len(listvals)):
        totimg=[]
        totrefimg=[]
        for j in range(1, navg):
            totimg.append(np.sum(img[i*navg+j]))
            totrefimg.append(np.sum(refimg[i*navg+j]))
        totimgnorm=(np.array(totimg)-bgmeanlst[i])/(np.array(totrefimg)-bgmeanreflst[i])
        meannormlst.append(np.mean(totimgnorm))
        meanlst.append(np.mean(totimg))
        meanreflst.append(np.mean(totrefimg))
        stdnormlst.append(np.std(totimgnorm))
        stdlst.append(np.std(totimg))
        stdreflst.append(np.std(totrefimg))

    plt.figure()
    plt.errorbar(np.array(listvals[1:]),meannormlst[1:],stdnormlst[1:],fmt='o')

def TemperatureGetter(ScanParameters, navg, filenumBG, filenumend):
    
    fnstop = filenumend
    fnstart = fnstop-len(ScanParameters)+1

    bgimagelst =getImages(filenumBG)

    tmpimg = np.zeros((np.shape(bgimagelst[0])[0], np.shape(bgimagelst[0])[1])) 
    for i in range(navg):
        tmpimg = tmpimg+ bgimagelst[i]

    bg = tmpimg




    imgagelst = []
    for fn in range(fnstart, fnstop+1):
        cc = getImages(fn)
        tmpimg = np.zeros((np.shape(cc[0])[0], np.shape(cc[0])[1])) 
        for i in range(navg):
            tmpimg = tmpimg+ cc[i]
        imgagelst.append(tmpimg-bg)

    plt.figure()
    PlotListOfImages(imgagelst)

    sigmaXlst = []
    sigmaYlst = []

    for i in range(len(imgagelst)):
        plt.figure()
        ydata = np.sum(imgagelst[i], axis = 0)
        xdata = range(len(ydata))
        popt, pcov, isFit = gaussianFitOffset(xdata,ydata)
        plt.subplot(1,2,1)
        plt.plot(ydata, 'ro')
        plt.plot(np.linspace(0,len(ydata), 1000),gaussianOffset(np.linspace(0,len(ydata), 1000),*popt))
        sigmaXlst.append(popt[2])


        ydata = np.sum(imgagelst[i], axis = 1)
        xdata = range(len(ydata))
        popt, pcov, isFit = gaussianFitOffset(xdata,ydata)
        plt.subplot(1,2,2)
        plt.plot(ydata, 'yo')
        plt.plot(np.linspace(0,len(ydata), 1000),gaussianOffset(np.linspace(0,len(ydata), 1000),*popt))
        sigmaYlst.append(popt[2])



    times = np.array(ScanParameters)*10E-6

    SigX = np.array(sigmaXlst)*8*6.45e-6*(1/0.47)
    SigY = np.array(sigmaYlst)*8*6.45e-6*(1/0.47)


    def linfunc(x, a, c):
        return a*x+c

    plt.figure()


    popt, pcov = curve_fit(linfunc, times**2, (SigX)**2)
    print('Temperature along x is '+str(int(((59*1.66e-27/(1.38e-23))*popt[0])/(1e-6)))+' muK')
    #plot fit along x
    plt.plot(times**2/(1e-6), (SigX)**2/(1e-6), 'r.' , label = 'x widths')
    plt.plot(np.linspace(0,max(times**2))/(1e-6), linfunc(np.linspace(0,max(times**2)),*popt)/(1e-6), 'r-', label = 'fit to x widths')
    #fit along y
    popt, pcov = curve_fit(linfunc, times**2, (SigY)**2)
    print('Temperature along y is '+str(int(((59*1.66e-27/(1.38e-23))*popt[0])/(1e-6)))+' muK')
    #plot fit along y 
    plt.plot(times**2/(1e-6), (SigY)**2/(1e-6), 'b.', label = 'y widths')
    plt.plot(np.linspace(0,max(times**2))/(1e-6), linfunc(np.linspace(0,max(times**2)),*popt)/(1e-6), 'b-', label = 'fit to y widths')
    plt.xlabel('squared expansion time in ms^2')
    plt.ylabel('squared gaussian RMS width mm^2')
    plt.legend()
    plt.show()
    
    
def GetPMTCountsFromTOF(listvals, navg, filenumBG, filenumend):
    
    filenumstart=filenumend-len(listvals)+1
    
    bgscans=getTOFScans(filenumBG)
    bgscansavg=np.mean(bgscans,axis=0)
    
    meancntslst=[]
    stdcountslst=[]
    for fn in range(filenumstart, filenumend+1):
        Tofscans=getTOFScans(fn)
        Tofscantotal=[]
        for Tofscan in Tofscans:
            Tofscantotal.append(np.sum(Tofscan-bgscansavg))
        meancntslst.append(np.mean(Tofscantotal))
        stdcountslst.append(np.std(Tofscantotal))
    
    plt.figure()
    plt.errorbar(listvals,meancntslst,stdcountslst,fmt='o')
    
    
def getTOFScans(fileNo):
    z=zipfile.ZipFile(dirPath + "\\" + fileNameString + "_" + str("%03d" % fileNo) +".zip",'r')
    scanlst=[]
    for f in z.namelist():
        if f[-3:]=='txt' and f[0]=='T':
            bytes=z.read(f)

            out = []
            buff = []
            for c in bytes:
                if c == '\r':
                    out.append(''.join(buff))
                    buff = []
                elif c == '\n':
                    continue
                else:
                    buff.append(c)
            else:
                if buff:
                   out.append(''.join(buff))
    
            outFloat=[]
    
            for item in out[1:]:
                outFloat.append(float(item))
            scanlst.append(outFloat)
    return scanlst
