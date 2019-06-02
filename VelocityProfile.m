%use after Timerseries
%% Function to calculate the trajectory information from the Unity and Camera timeseries data
%return: Info = {max velocity, direction, distance to max velocity}
%param: Unity position, Unity velocity, Camera position, Camera velocity
function [UnityInfo, RealInfo] = VelocityProfile(upos,uvel,cpos,cvel)


trials = 0; %good trials
for t=1:length(cpos)
    
    %convert timeseries data to vectors for each trial
    xu = squeeze(upos{t}.data(1,:,:)); xdotu = squeeze(uvel{t}.data(1,:,:));
    zu = squeeze(upos{t}.data(2,:,:)); zdotu = squeeze(uvel{t}.data(2,:,:));
    xc = squeeze(cpos{t}.data(1,:,:)); xdotc = squeeze(cvel{t}.data(1,:,:));
    zc = squeeze(cpos{t}.data(2,:,:)); zdotc = squeeze(cvel{t}.data(2,:,:));
    time = uvel{t}.time;
    
    %bad trial
    mag = (xdotc.^2 + zdotc.^2) .^(0.5);
    if (isnan(mag(1)))
        continue;
    end
    
    trials = trials+1; %good trial
    
    
    %Unity
    mag = (xdotu.^2 + zdotu.^2) .^(0.5); %magnitude of velocity vector
    magidx = find(mag==max(mag)); magidx = magidx(1); 
    tmax = find(time == time(find(magidx))); %time at max velocity
    maxvelu(trials) = max(mag);
    directionu(trials) = getDegrees(xu,zu);
    distu(trials) = (xu(tmax)^2 + zu(tmax)^2)^0.5;
    
    %Camera
    mag = (xdotc.^2 + zdotc.^2) .^(0.5); %magnitude of velocity vector
    magidx = find(mag==max(mag)); magidx = magidx(1); %time at max velocity
    tmax = find(time == time(find(magidx)));
    maxvelc(trials) = max(mag);
    directionc(trials) = getDegrees(xc,zc);
    distc(trials) = (xc(tmax)^2 + zc(tmax)^2)^0.5;
    
    
end

UnityInfo.velocity = maxvelu;
UnityInfo.theta = directionu;
UnityInfo.distance = distu;

RealInfo.velocity = maxvelc;
RealInfo.theta = directionc;
RealInfo.distance = distc;

    %Function to return the angle of the shot from the velocity vectors
    function angle = getDegrees(x,z)
        deg = (atan(z./x)) * 180/pi;
        %add 180 degrees if negative to match axis
        addpi=deg < 0;
        deg(addpi) = deg(addpi) + 180;
        angle = deg(length(deg)/2);
        if (isnan(angle))
            angle = 0;
        end
    end

end