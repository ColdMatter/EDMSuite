@echo off
Doskey aom_dds=serial_to_dds_gw.exe C:\Code\ukus_dds_comm_gw\ukus_dds_aom_conf.txt comm 21
Doskey aom_dds_verbose=serial_to_dds_gw.exe C:\Code\ukus_dds_comm_gw\ukus_dds_aom_conf.txt comm 21 verbose
Doskey slaves_dds=serial_to_dds_gw.exe C:\Code\ukus_dds_comm_gw\ukus_dds_slaves_conf.txt comm 19
Doskey slaves_dds_verbose=serial_to_dds_gw.exe C:\Code\ukus_dds_comm_gw\ukus_dds_slaves_conf.txt comm 19 verbose