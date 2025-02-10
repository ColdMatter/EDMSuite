// console/misc/common.h
//

void dcamcon_show_dcamerr( HDCAM hdcam, DCAMERR errid, const char* apiname, const char* fmt=0, ...  );

HDCAM dcamcon_init_open();
void dcamcon_show_dcamdev_info( HDCAM hdcam );
