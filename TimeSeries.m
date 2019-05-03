function [unityPos, unityVel, cameraPos, cameraVel] = TimeSeries(UD,CD,duration)
tunity=UD{1}; xunity=UD{2}; zunity=UD{3}; xdot=UD{4}; zdot=UD{5};
tcam=CD{1}; xcam=CD{2}; zcam=CD{3};

unityPos={}; unityVel={}; cameraPos={}; cameraVel={};
window = 0.05; %seconds
%duration = 2.5; %seconds
for tr=1:47
    
    %duration = floor(min(tunity{tr}(end), tcam{tr}(end)));
    bins = duration / window;
    newtime=[-0.5:window:duration-window];
    
    %resample
    tsunity = timeseries([xunity{tr}; zunity{tr}],tunity{tr});
    tscam = timeseries([xcam{tr}; zcam{tr}],tcam{tr});
    newunity = resample(tsunity, newtime);
    newcam = resample(tscam,newtime);
    
    %velocities
    dtunity = timeseries([xdot{tr};zdot{tr}],tunity{tr});
    newvelunity = resample(dtunity, newtime);
    xdotcam = diff(squeeze(newcam.data(1,:,:))) ./ diff(newcam.time);
    zdotcam = diff(squeeze(newcam.data(2,:,:))) ./ diff(newcam.time);
    dtcam = timeseries([xdotcam'; zdotcam'],newtime(1:end-1));
    newvelcam = resample(dtcam, newtime);
    
    
    unityPos{tr} = newunity; unityVel{tr} = newvelunity;
    cameraPos{tr} = newcam; cameraVel{tr} = newvelcam;
    
    hold on
    plot(squeeze(newunity.data(1,:,:)), squeeze(newunity.data(2,:,:)), 'linewidth',2)
    plot(squeeze(newcam.data(1,:,:)), squeeze(newcam.data(2,:,:)), 'linewidth',2)
    
    clf
    
end

end