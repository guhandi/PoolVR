function [xdata, zdata] = UnityCoord(data)
%corner1pos = (-0.3604, 0.8554, -0.9259)
%corner2pos = (0.3342, 0.8554, -0.9259)
%corner5pos = (-0.3604, 0.8554, 0.4634)
%corner6pos = (0.3342, 0.8554, 0.4634)

% load data
trial = grp2idx(table2array(data(:,1)));
time = table2array(data(:,2));
cbPos = table2array(data(:,6));
cbVel = table2array(data(:,7));

[cbXpos,cbYpos,cbZpos] = categoryToVector(cbPos);
[cbXvel,cbYvel,cbZvel] = categoryToVector(cbVel);

pocketx = [-0.3604,0.3342,-0.3604,0.3342,-0.3604,0.3342];
pocketz = [-0.9259,-0.9259,-0.2312,-0.2312,0.4634,0.4634];
w = abs(pocketx(1) - pocketx(2));
us = 1/w;
xstart = pocketx(1);
zstart = pocketz(1);
px = pocketx - xstart;
pz = pocketz - zstart;

xdata = {};
zdata = {};
thetadata = {};
thetaddata = {};

totaltime=0;
%for t=1:trial(end)-1
for t=1:48

trialnum = t;
idx = find(trial == trialnum);
trtime = time(idx);
totaltime = totaltime + max(trtime);

x = us * (cbXpos(idx)-xstart); %x position data for specific trial
y = cbYpos(idx);
z = us * (cbZpos(idx)-zstart);

xdot = cbXvel(idx);
ydot = cbYvel(idx);
zdot = cbZvel(idx);

theta = atan(z./x);
thetad = atan(zdot./xdot);
theta(isnan(theta)) = 0;
thetad(isnan(thetad)) = 0;

xdata{t} = x;
zdata{t} = z;
thetadata{t} = theta;
thetaddata{t} = thetad;


end


end
