# -*- coding: utf-8 -*-
"""
Created on Wed Jan 23 11:34:37 2019

@author: CaFMOT
"""

import numpy as np
import matplotlib.pyplot as plt
import zipfile,os,json

class Signal_plotter():
    def __init__(self,args):
        filenames=args['filenames']
        dirpath=args['dirpath']
        self.plottypes=args['plottypes']
        analog_dicts=[]
        digital_dicts=[]
        for filename in filenames:
            filepath=os.path.join(dirpath,filename)        
            src=zipfile.ZipFile(filepath+'.zip','r')
            analog_dict=json.loads(src.read(filename+'_analogPattern.json'))
            digital_dict=json.loads(src.read(filename+'_digitalPattern.json'))
            analog_dicts.append(analog_dict)
            digital_dicts.append(digital_dict)
        self.analog_dicts=analog_dicts
        self.digital_dicts=digital_dicts

    def analog_sort(self,in_dict):
        out_dict={}
        tmax=0
        key_list=[]
        for key,val in in_dict.iteritems():
            out_dict[key]={}
            out_dict[key]['t']=np.array(val.keys(),dtype=int)
            out_dict[key]['V']=np.array(val.values(),dtype=float)
            if len(out_dict[key]['t'])>2:
                sorted_list=out_dict[key]['t'].argsort() 
                out_dict[key]['t']=out_dict[key]['t'][sorted_list]
                out_dict[key]['V']=out_dict[key]['V'][sorted_list]
                tmax=np.max([tmax,np.max(out_dict[key]['t'])])
                key_list.append(key)
        return out_dict,tmax,key_list
    
    def analog_fill(self,in_dict,tmax,key_list):       
        out_dict={}
        for key,val in in_dict.iteritems():
            if key in key_list:
                out_dict[key]=np.zeros((tmax))
                for i in range(len(val['t'])-1):
                    dt=val['t'][i+1]-val['t'][i]
                    if dt==1:
                        out_dict[key][val['t'][i]]=val['V'][i]
                    else:
                        out_dict[key][val['t'][i]:val['t'][i+1]]=val['V'][i]*np.ones(dt)
        return out_dict

    def analog_iter(self,analog_dict):
        analog_sorted_dict,tmax,key_list=self.analog_sort(analog_dict)
        self.analog_filled_dicts.append(self.analog_fill(analog_sorted_dict,tmax,key_list))
        self.analog_ts.append(np.arange(0,tmax))
        self.analog_key_lists.append(key_list)
        
    def analog_plot(self):
        fig,axes=plt.subplots(len(self.analog_key_lists[0]),1,figsize=(10,8),sharex=True)
        fig.suptitle('Analog Pattern')
        for j,analog_dict in enumerate(self.analog_filled_dicts):
            for i,key in enumerate(self.analog_key_lists[0]):
                axes[i].plot(self.analog_ts[j]/100.0,analog_dict[key],self.plottypes[j])
                axes[i].yaxis.set_label_position("right")
                axes[i].set_ylabel(self.analog_key_lists[0][i],fontsize=10,fontweight='bold')
        axes[len(self.analog_key_lists[0])-1].set_xlabel('time [ms]')
        plt.show()

    def digital_sort(self,in_dict):
        pattern=in_dict['pattern']
        channels=in_dict['channels']
        line_split_list=pattern.split('\n')[1:-1]
        ttime=[]
        out_dict={}
        key_list=[]
        for key,val in channels.iteritems():
            out_dict[key]=[]
        for i,item in enumerate(line_split_list):
            tab_split_list=item.split('\t')
            ttime.append(tab_split_list[0])
            items=tab_split_list[1:-1]
            for key,val in channels.iteritems():
                out_dict[key].append(items[val])
        for key,val in out_dict.iteritems():
            c=0
            for i,t in enumerate(ttime):
                if i==0:
                    if val[i]=='-':
                        val[i]='0'
                    elif val[i]=='U':
                        val[i]='5.00'
                        c+=1
                else:
                    if val[i]=='-' and val[i-1]=='5.00':
                        val[i]='5.00'
                        c+=1
                    elif val[i]=='-' and val[i-1]=='4.99':
                        val[i]='0'
                        c+=1
                    elif val[i]=='-':
                        val[i]='0'
                    elif val[i]=='U':
                        val[i]='5.00'
                        c+=1
                    elif val[i]=='D':
                        val[i]='4.99'
                        c+=1
            if c>0:
                key_list.append(key)
            out_dict[key]=np.ceil(np.array(out_dict[key],dtype=float))
        return out_dict,np.array(ttime,dtype=int),key_list

    def digital_fill(self,in_dict,ttime,key_list):
        out_dict={}
        for key,val in in_dict.iteritems():
            new_val=[]
            tall=[]
            if key in key_list:
                for i,t in enumerate(ttime):
                    if i<(len(ttime)-1):
                        if val[i]<2.5 and val[i+1]>2.5:
                            tall.append(ttime[i])
                            new_val.append(val[i])
                            tall.append(ttime[i+1])
                            new_val.append(0)
                        elif val[i]>2.5 and val[i+1]<2.5:
                            tall.append(ttime[i])
                            new_val.append(val[i])
                            tall.append(ttime[i])
                            new_val.append(0)
                        else:
                            tall.append(ttime[i])
                            new_val.append(val[i])
            out_dict[key]=[new_val,tall]
        return out_dict

    def digital_iter(self,digital_dict):
        digital_sorted_dict,ttime,key_list=self.digital_sort(digital_dict)
        out_dict=self.digital_fill(digital_sorted_dict,ttime,key_list)
        self.digital_filled_dicts.append(out_dict)
        self.digital_key_lists.append(key_list)        

    def digital_plot(self):
        fig,axes=plt.subplots(len(self.digital_key_lists[0]),1,figsize=(10,24),sharex=True)
        fig.suptitle('Digital Pattern')
        for j,digital_dict in enumerate(self.digital_filled_dicts):
            for i,key in enumerate(self.digital_key_lists[0]):
                axes[i].plot(digital_dict[key][1],digital_dict[key][0],self.plottypes[j])
                axes[i].yaxis.set_label_position("right")
                axes[i].set_ylabel(self.digital_key_lists[0][i],fontsize=5,fontweight='bold')
        axes[len(self.digital_key_lists[0])-1].set_xlabel('time [ms]')
        plt.show()
    
    def run(self):
        self.analog_filled_dicts=[]
        self.digital_filled_dicts=[]
        self.analog_ts=[]
        self.analog_key_lists=[]
        self.digital_ts=[]
        self.digital_key_lists=[]
        for analog_dict,digital_dict in zip(self.analog_dicts,self.digital_dicts):
            self.analog_iter(analog_dict)
            self.digital_iter(digital_dict)
        self.analog_plot()
        self.digital_plot()
        
        

        

if __name__=='__main__':
    args={}
    args['filenames']=['CaF11Feb1900_296','CaF13Nov1800_100']
    args['dirpath']='C:\Users\cafmot\Box\CaF MOT\MOTData\MOTMasterData'
    args['plottypes']=['-ok','-or']
    sp=Signal_plotter(args)
    sp.run()
    
    

    