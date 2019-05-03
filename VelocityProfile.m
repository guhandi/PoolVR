
%max velocity
%dist to max velocity
%direction

for t=1:47
    xu = squeeze(upos{t}.data(1,:,:)); xdotu = squeeze(uvel{t}.data(1,:,:));
    zu = squeeze(upos{t}.data(2,:,:)); zdotu = squeeze(uvel{t}.data(2,:,:));
    xc = squeeze(cpos{t}.data(1,:,:)); xdotc = squeeze(cvel{t}.data(1,:,:));
    zc = squeeze(cpos{t}.data(2,:,:)); zdotc = squeeze(cvel{t}.data(2,:,:));
    time = uvel{t}.time;
    
    %Unity
%     mag = (xdotu.^2 + zdotu.^2) .^(0.5);
%     tmax = find(time == time(find(mag == max(mag))));
%     degrees = (atan(zdotu(tmax) / xdotu(tmax))) * 180/pi;
%     if (degrees < 0)
%         degrees = 180 + degrees;
%     end
%     maxvelu(t) = max(mag)
%     directionu(t) = degrees
%     distu(t) = (xu(tmax)^2 + zu(tmax)^2)^0.5
    
    %Camera
    mag = (xdotc.^2 + zdotc.^2) .^(0.5);
    tmax = find(time == time(find(mag == max(mag))));
    degrees = (atan(zdotc(tmax) / xdotc(tmax))) * 180/pi;
    if (degrees < 0)
        degrees = 180 + degrees;
    end
    maxvelc(t) = max(mag)
    directionc(t) = degrees
    distc(t) = (xc(tmax)^2 + zc(tmax)^2)^0.5
    
    
   
    
end