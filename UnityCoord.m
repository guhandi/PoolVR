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

%pocketx = [-0.3604,0.3342,-0.3604,0.3342,-0.3604,0.4634]; %val9&10
%pocketz = [-0.9259,-0.9259,-0.2313,-0.2313,0.3409,0.4634];
pocketx = [-0.3409,0.3560,-0.3409,0.3560,-0.3409,0.3560]; %val vtwo
pocketz = [-0.9409,-0.9409,-0.3,-0.3,0.3409,0.3409];
w = abs(pocketx(1) - pocketx(2));
us = 1/w;
xstart = cbXpos(1);
zstart = cbZpos(1);

tr=0;
for t=1:trial(end)-1


trialnum = t;
idx = find(trial == trialnum);
trtime = time(idx);
if (trtime(end) < 1)
    continue;
end
tr=tr+1;

x = us * (cbXpos(idx)-xstart); %x position data for specific trial
z = us * (cbZpos(idx)-zstart);
xdot{tr} = cbXvel(idx);
zdot{tr} = cbZvel(idx);

%shift time
idx = find(abs(zdot{tr}) > 0.001);
if (length(idx) == 0)
    idx(1) = 1;
end
start = idx(1);
trtime = trtime - trtime(start);
trtime = trtime';

xdata{tr} = x;
zdata{tr} = z;
timedata{tr} = trtime;

end

UnityData = {timedata; xdata; zdata; xdot; zdot};

end
