%after Timerseries

function [UnityInfo, RealInfo] = VelocityProfile(upos,uvel,cpos,cvel)

%max velocity
%dist to max velocity
%direction
trials = 0;
for t=1:46
    
    
    trials = trials+1;
    
    xu = squeeze(upos{t}.data(1,:,:)); xdotu = squeeze(uvel{t}.data(1,:,:));
    zu = squeeze(upos{t}.data(2,:,:)); zdotu = squeeze(uvel{t}.data(2,:,:));
    xc = squeeze(cpos{t}.data(1,:,:)); xdotc = squeeze(cvel{t}.data(1,:,:));
    zc = squeeze(cpos{t}.data(2,:,:)); zdotc = squeeze(cvel{t}.data(2,:,:));
    time = uvel{t}.time;
    
    %unity
    degrees = (atan(zu./xu)) * 180/pi;
    tid=degrees < 0;
    degrees(tid) = degrees(tid) + 180;
    thetau = degrees(length(degrees)/2);
    if (isnan(thetau))
        thetau = 0;
    end
    
    
    %cam
    degrees = (atan(zc./xc)) * 180/pi;
    tid=degrees < 0;
    degrees(tid) = degrees(tid) + 180;
    thetac = degrees(length(degrees)/2);
    if (isnan(thetac))
        thetac = 0;
    end
    
    %Unity
    mag = (xdotu.^2 + zdotu.^2) .^(0.5);
    magidx = find(mag==max(mag)); magidx = magidx(1);
    tmax = find(time == time(find(magidx)));
    %tmax = find(time == time(find(mag == max(mag))));
    maxvelu(trials) = max(mag);
    directionu(trials) = thetau;
    distu(trials) = (xu(tmax)^2 + zu(tmax)^2)^0.5;
    
    %Camera
    mag = (xdotc.^2 + zdotc.^2) .^(0.5);
    if (isnan(mag(1)))
        continue;
    else
        magidx = find(mag==max(mag)); magidx = magidx(1);
        tmax = find(time == time(find(magidx)));
        %tmax = find(time == time(find(mag == max(mag))));
    end
    maxvelc(trials) = max(mag);
    directionc(trials) = thetac;
    distc(trials) = (xc(tmax)^2 + zc(tmax)^2)^0.5;
    
    
end

UnityInfo.velocity = maxvelu;
UnityInfo.theta = directionu;
UnityInfo.distance = distu;

RealInfo.velocity = maxvelc;
RealInfo.theta = directionc;
RealInfo.distance = distc;

end