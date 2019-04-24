%function mse = MeanError(x, z, xcam, xcam)
pocketx = [-0.3604,0.3342,-0.3604,0.3342,-0.3604,0.3342];
pocketz = [-0.9259,-0.9259,-0.2312,-0.2312,0.4634,0.4634];
w = abs(pocketx(1) - pocketz(1));
us = 0.5/w;

%shift
xstart = pocketx(1);
zstart = pocketz(1);
pocketx = pocketx - xstart;
pocketz = pocketz - zstart;



figure
hold on
for t=1:3

%subplot(2,1,1)
plot(xunity{t},zunity{t}, 'linewidth',2)


%subplot(2,1,2)
%plot(xcam{t},zcam{t}, 'linewidth',2)


end




%end