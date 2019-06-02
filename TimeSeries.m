%Use after UnityCoor.m and camera.m
%% Function to resample the Unity and Camera data so they are on the same time scale
%return: position and velocity timeseries data from t=0 to t=duration for Unity and Camera
%param: Unity data, Camera data, duration (seconds)
function [unityPos, unityVel, cameraPos, cameraVel] = TimeSeries(UD,CD,duration)
tunity=UD{1}; xunity=UD{2}; zunity=UD{3}; xdot=UD{4}; zdot=UD{5};
tcam=CD{1}; xcam=CD{2}; zcam=CD{3};

unityPos={}; unityVel={}; cameraPos={}; cameraVel={};
fr = 0.05; %resampling rate (seconds)
%duration = 0.5; %seconds

%Plotting
px = [-0.5,0.5,-0.5,0.5,-0.5,0.5];  
pz = [-0.5, -0.5, 0.5, 0.5, 1.5, 1.5];
borderx = [px(1), px(5), px(6), px(2), px(1)];
borderz = [pz(1), pz(5), pz(6), pz(2), px(1)];


for tr=1:length(CD{1})
    
    newtime=0:fr:(duration-fr); %new time scale
    
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
    
%     %Plotting
%     subplot(2,1,2)
%     hold on
%     plot(squeeze(newvelunity.data(1,:,:)), squeeze(newvelunity.data(2,:,:)), 'linewidth',2)
%     plot(squeeze(newvelcam.data(1,:,:)), squeeze(newvelcam.data(2,:,:)), 'linewidth',2)
%     plot(px,pz,'ko', 'linewidth', 3, 'HandleVisibility','off');
%     plot(borderx,borderz,'k', 'linewidth',2, 'HandleVisibility','off');
%     plot(squeeze(newunity.data(1,:,:)), squeeze(newunity.data(2,:,:)), 'linewidth',2)
%     plot(squeeze(newcam.data(1,:,:)), squeeze(newcam.data(2,:,:)), 'linewidth',2)
%     xlabel('x')
%     ylabel('z');
%     title('First 1 sec trajectories')
%     
%     subplot(2,1,1)
%     hold on
%     plot(px,pz,'ko', 'linewidth', 3, 'HandleVisibility','off');
%     plot(borderx,borderz,'k', 'linewidth',2, 'HandleVisibility','off');
%     plot(xunity{tr},zunity{tr}, 'linewidth',2)
%     plot(xcam{tr},zcam{tr}, 'r', 'linewidth',2)
%     %plot(zcam{tr}, 'r', 'linewidth',2)
%     xlabel('x')
%     ylabel('z');
%     title('Cleaned & scaled trajectories')
%     legend('unity', 'real-world')
%     
%     clf
    
end

end