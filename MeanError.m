%function mse = MeanError(x, z, xcam, xcam)
pocketx = [-0.3604,0.3342,-0.3604,0.3342,-0.3604,0.3342];
pocketz = [-0.9259,-0.9259,-0.2312,-0.2312,0.4634,0.4634];

w = abs(pocketx(1) - pocketx(2));
us = 1/w;

%shift unity
xstart = pocketx(1);
zstart = pocketz(1);
px = us * (pocketx - xstart);
pz = us * (pocketz - zstart);
borderx = [px(1), px(5), px(6), px(2), px(1)];
borderz = [pz(1), pz(5), pz(6), pz(2), px(1)];


%shift camera
xo = xcam{1}(1);
zo = zcam{1}(1);

figure
hold on
for t=1:48

%Unity
%subplot(2,1,1)
plot(px,pz,'ko', 'linewidth', 3, 'HandleVisibility','off');
plot(borderx,borderz,'k', 'linewidth',2, 'HandleVisibility','off');
plot(xunity{t},zunity{t}, 'linewidth',2)
xlabel('x')
ylabel('z');
title('Cue-ball trajectory in unity')


%Camera
%subplot(2,1,2)
plot(xcam{t},zcam{t}, 'r', 'linewidth',2)
xlabel('x')
ylabel('z');
title('Cue-ball trajectories')



clf

end




%end