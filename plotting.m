gr = [4,11,19,43];
px = [-0.5,0.5,-0.5,0.5,-0.5,0.5];
pz = [-0.5, -0.5, 0.5, 0.5, 1.5, 1.5];
borderx = [px(1), px(5), px(6), px(2), px(1)];
borderz = [pz(1), pz(5), pz(6), pz(2), px(1)];


%% Velocity Profiles
figure()
duration=2.5; %seconds
for t=1:47
    xu = squeeze(upos{t}.data(1,:,:)); xdotu = squeeze(uvel{t}.data(1,:,:));
    zu = squeeze(upos{t}.data(2,:,:)); zdotu = squeeze(uvel{t}.data(2,:,:));
    xc = squeeze(cpos{t}.data(1,:,:)); xdotc = squeeze(cvel{t}.data(1,:,:));
    zc = squeeze(cpos{t}.data(2,:,:)); zdotc = squeeze(cvel{t}.data(2,:,:));
    
    subplot(2,1,1)
    hold on
    plot(xu, zu, 'linewidth',2)
    plot(xc, zc, '--','linewidth',2)
    xlabel('x');
    ylabel('z');
    title('Unity vs Real-World Trajectory')
    legend('Unity','Real-World')
    
    subplot(2,1,2)
    hold on
    plot(xdotu, zdotu, 'linewidth',2)
    plot(xdotc, zdotc, '--','linewidth',2)
    xlabel('xdot');
    ylabel('zdot');
    title('Unity vs Real-World Velocity')
    legend('Unity','Real-World')
    
    
    clf
    
end

%% Unit Vector Plot
% figure()
% hold on
% plot([0,0],[-1,1],'k','linewidth',0.5,'HandleVisibility','off');
% plot([-1,1],[0,0],'k','linewidth',0.5,'HandleVisibility','off');
% axis equal
% r = 1;
% colors = hsv(length(gr));
% 
% for k=1:length(gr)
%     j = gr(k);
%     %color = [rand(1), rand(1), rand(1)];
%     color = colors(k,:);
%     
%     %unity
%     a = thetau{j};
%     xu = r*cos(a);
%     zu = abs(r*sin(a));
%     ou = zeros(size(a));
% 
%     %cam
%     b = thetac{j};
%     xc = r*cos(b);
%     zc = abs(r*sin(b));
%     oc = zeros(size(b));
%     
%     plot([ou' xu']', [ou' zu']', 'Color', color, 'linewidth', 2)
%     plot([oc' xc']', [oc' zc']', '--','Color', color,'linewidth', 2)
%     
% 
% 
% end
% xlabel('x');
% ylabel('y')
% title('Unit vectors for first 0.25s of trajctory')
% legend('trial4 unity', 'trial4 real-world','trial11 unity', 'trial11 real-world','trial19 unity', 'trial19 real-world','trial43 unity', 'trial43 real-world');

%For vector - trajectory comparison

% figure()
% hold on
% axis([-0.5,0.5, -0.5, 1.5])
% r = 1;
% colors = hsv(length(gr));
% for k=1:length(gr)
%     j=gr(k);
%     color = colors(k,:);
%     c1=[0.4,0,0.9];
%     
%     %unity
%     a = thetau{j};
%     xvu = r*cos(a);
%     zvu = abs(r*sin(a));
%     ou = zeros(size(a));
% 
%     %cam
%     b = thetac{j};
%     xvc = r*cos(b);
%     zvc = abs(r*sin(b));
%     oc = zeros(size(b));
%     
%     hold on
%     plot(px,pz,'ko', 'linewidth', 3, 'HandleVisibility','off');
%     plot(borderx,borderz,'k', 'linewidth',2, 'HandleVisibility','off');
%     plot([ou' xvu']', [ou' zvu']', 'Color', c1, 'linewidth', 2)
%     plot(xunity{j}, zunity{j}, '--','Color', c1,'linewidth', 2)
%     plot([oc' xvc']', [oc' zvc']', 'Color', 'r', 'linewidth', 2)
%     plot(xcam{j}, zcam{j}, '--','Color', 'r','linewidth', 2)
%  
%     
%     title('Actual trajectory vs derived initial direction') 
% 
%     clf
% 
% end

%% MSE Plot
MSEred = MSE;
MSEred(MSEred > (pi/2)^2) = [];
figure
plot(MSE,'linewidth',2)
xlabel('trial');
ylabel('Mean Squared Error (rad)')
title('MSE between initial 0.25s trajectory for all trials')





% %% Trajectory plots
% for t=43:47
% 
% %Unity raw
% subplot(2,2,1)
% plot(urawx{t},urawz{t}, 'linewidth',2)
% xlabel('x')
% ylabel('z');
% title('Raw data: Cue-ball trajectories in Unity')
% 
% 
% %%Camera raw
% subplot(2,2,2)
% plot(rawx{t},rawz{t}, 'r', 'linewidth',2)
% xlabel('x')
% ylabel('z');
% title('Raw data: Cue-ball trajectories in real life')
% 
% %both scaled
% subplot(2,2,3)
% hold on
% plot(px,pz,'ko', 'linewidth', 3, 'HandleVisibility','off');
% plot(borderx,borderz,'k', 'linewidth',2, 'HandleVisibility','off');
% plot(xunity{t},zunity{t}, 'linewidth',2)
% plot(xcam{t},zcam{t}, 'r', 'linewidth',2)
% xlabel('x')
% ylabel('z');
% title('Cleaned & scaled trajectories')
% legend('unity', 'real-world')
% 
% %first 0.5 seconds
% subplot(2,2,4)
% hold on
% plot(px,pz,'ko', 'linewidth', 3, 'HandleVisibility','off');
% plot(borderx,borderz,'k', 'linewidth',2, 'HandleVisibility','off');
% plot(xu{t},zu{t}, 'linewidth',2)
% plot(xc{t},zc{t}, 'r', 'linewidth',2)
% xlabel('x')
% ylabel('z');
% title('First 0.5s trajectories after collision')
% legend('unity', 'real-world')
% 
% 
% clf
% 
% end
