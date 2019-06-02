% %Get unity data
% pocketx = [-0.3604,0.3342,-0.3604,0.3342,-0.3604,0.4634]; %val9&10
% %pocketx = [-0.3409,0.3560,-0.3409,0.3560,-0.3409,0.3560]; %val vtwo
% 
% UD = UnityCoord(Validation9,pocketx);
% CD = camera(PILOTGS9042420191928position, PILOTGS9042420191928beginTrial);
% [upos,uvel,cpos,cvel] = TimeSeries(UD,CD,0.5);
% [UnityInfo, RealInfo] = VelocityProfile(upos,uvel,cpos,cvel);


%% plots

%uvel = UnityInfo.velocity; utheta = UnityInfo.theta; udist = UnityInfo.distance;
%rvel = RealInfo.velocity; rtheta = RealInfo.theta; rdist = RealInfo.distance;

%Validation 9&10 (version 1)
% uvel = [Valvone9results.Unity.velocity, Valvone10results.Unity.velocity];
% utheta = [Valvone9results.Unity.theta, Valvone10results.Unity.theta];
% udist = [Valvone9results.Unity.distance, Valvone10results.Unity.distance];
% 
% rvel = [Valvone9results.Real.velocity, Valvone10results.Real.velocity];
% rtheta = [Valvone9results.Real.theta, Valvone10results.Real.theta];
% rdist = [Valvone9results.Real.distance, Valvone10results.Real.distance];

%Validation final (version 2)
uvel = [Valvtwo3results.Unity.velocity, Valvtwo4results.Unity.velocity];
utheta = [Valvtwo3results.Unity.theta, Valvtwo4results.Unity.theta];
udist = [Valvtwo3results.Unity.distance, Valvtwo4results.Unity.distance];

rvel = [Valvtwo3results.Real.velocity, Valvtwo4results.Real.velocity];
rtheta = [Valvtwo3results.Real.theta, Valvtwo4results.Real.theta];
rdist = [Valvtwo3results.Real.distance, Valvtwo4results.Real.distance];


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