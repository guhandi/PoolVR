UD = UnityCoord(validationvtwo4);
CD = camera(validationavrv2t4052920191435position, validationavrv2t4052920191435beginTrial);
[upos,uvel,cpos,cvel] = TimeSeries(UD,CD,0.5);
[UnityInfo, RealInfo] = VelocityProfile(upos,uvel,cpos,cvel);


%% plots

uvel = UnityInfo.velocity;
utheta = UnityInfo.theta;
udist = UnityInfo.distance;
rvel = RealInfo.velocity;
rtheta = RealInfo.theta;
rdist = RealInfo.distance;

% uvel = [Val9Result.Unity.velocity, Val10Results.Unity.velocity];
% utheta = [Val9Result.Unity.theta, Val10Results.Unity.theta];
% udist = [Val9Result.Unity.distance, Val10Results.Unity.distance];
% 
% rvel = [Val9Result.Real.velocity, Val10Results.Real.velocity];
% rtheta = [Val9Result.Real.theta, Val10Results.Real.theta];
% rdist = [Val9Result.Real.distance, Val10Results.Real.distance];

MSEv = sqrt(sum((uvel - rvel).^2));
MSEt = sqrt(sum((utheta - rtheta).^2));
MSEd = sqrt(sum((udist - rdist).^2));

figure

subplot(2,2,1)
hold on
title('Max Velocity for each trial')
xlabel('trial')
ylabel('velocity')
plot(uvel,'linewidth',2)
plot(rvel,'linewidth',2)
legend('Unity','Real')

subplot(2,2,2)
hold on
title('Direction for each trial')
xlabel('trial')
ylabel('degrees')
plot(utheta,'linewidth',2)
plot(rtheta,'linewidth',2)

subplot(2,2,3)
hold on
title('Distance at max velocity for each trial')
xlabel('trial')
ylabel('distance')
plot(udist,'linewidth',2)
plot(rdist,'linewidth',2)

subplot(2,2,4)
MSE = {'Velocity', MSEv;'Theta',MSEt;'Distance',MSEd}
bar([MSE{:,2}])
set(gca,'XtickLabel',MSE(:,1))
title('MSE')
ylabel('MSE')