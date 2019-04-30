function [unityPos, unityVel, cameraPos, cameraVel] = TimeSeries(tunity,xunity,zunity,xdot,zdot,tcam, xcam,zcam)
unityPos={}; unityVel={}; cameraPos={}; cameraVel={};
window = 0.05; %seconds
for tr=1:47
    
    trialtime = floor(min(tunity{tr}(end), tcam{tr}(end)));
    bins = trialtime / window;
    newtime=[-0.5:window:trialtime-window];
    
    %resample
    tsunity = timeseries([xunity{tr}; zunity{tr}],tunity{tr});
    tscam = timeseries([xcam{tr}; zcam{tr}],tcam{tr});
    newunity = resample(tsunity, newtime);
    newcam = resample(tscam,newtime);
    
    %velocities
    dtunity = timeseries([xdot{tr};zdot{tr}],tunity{tr});
    newvelunity = resample(dtunity, newtime);
    velcam = diff(squeeze(newcam.data))./diff(newcam.time);
    
    unityPos{tr} = newunity; unityVel{tr} = newvelunity;
    cameraPos{tr} = newcam; cameraVel{tr} = velcam;
    
    %hold on
    %view(3)
    %plot3(newcam.time, squeeze(newcam.data(1,:,:)), squeeze(newcam.data(2,:,:)))
    %plot3(newunity.time, squeeze(newunity.data(1,:,:)), squeeze(newunity.data(2,:,:)))
    
    %clf
    
end

end