function UnityData = UnityCoord(data)
%corner1pos = (-0.3604, 0.8554, -0.9259)
%corner2pos = (0.3342, 0.8554, -0.9259)
%corner5pos = (-0.3604, 0.8554, 0.4634)
%corner6pos = (0.3342, 0.8554, 0.4634)

timedata={}; xdata={}; zdata={}; xdot={}; zdot={};
UnityData = {};

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
w = 0.6946;
us = 1/w;
xstart = cbXpos(1);
zstart = cbZpos(1);

%for t=1:trial(end)-1
for t=1:25

trialnum = t;
idx = find(trial == trialnum);
trtime = time(idx);

x = us * (cbXpos(idx)-xstart); %x position data for specific trial
z = us * (cbZpos(idx)-zstart);
xdot{t} = cbXvel(idx);
zdot{t} = cbZvel(idx);

%shift time
idx = find(abs(zdot{t}) > 0.001);
start = idx(1);
trtime = trtime - trtime(start);
trtime = trtime';

xdata{t} = x;
zdata{t} = z;
timedata{t} = trtime;

end

UnityData = {timedata; xdata; zdata; xdot; zdot};

end
