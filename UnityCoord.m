%% Function to process the Unity Text data
%return UnityData = {time, x position, z position, x velocity, z velocity}
%param: data, unity pocket x position vector (for scaling)
function UnityData = UnityCoord(data,pocketx)

%Unity variables to return
timedata={}; xdata={}; zdata={}; xdot={}; zdot={};
UnityData = {};

% load data
trial = grp2idx(table2array(data(:,1)));
time = table2array(data(:,2));
cbPos = table2array(data(:,6));
cbVel = table2array(data(:,7));

%get X and Z position and velocity
[cbXpos,cbYpos,cbZpos] = categoryToVector(cbPos);
[cbXvel,cbYvel,cbZvel] = categoryToVector(cbVel);

%scale factor us to make wifht of table 0.5
w = abs(pocketx(1) - pocketx(2));
us = 1/w;

%intial position of cueball
xstart = cbXpos(1);
zstart = cbZpos(1);

%go through data for each trial
tr=0; %good trial number
for t=1:trial(end)-1


trialnum = t;
idx = find(trial == trialnum);
trtime = time(idx);
if (trtime(end) < 1) %if trial time less than 1 second then consider "bad" trial
    continue;
end
tr=tr+1;

x = us * (cbXpos(idx)-xstart); %x position data for specific trial
z = us * (cbZpos(idx)-zstart); %z position data for specific trial
xdot{tr} = cbXvel(idx);
zdot{tr} = cbZvel(idx);

%shift time so cue stick - cue ball collision is at t=0
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
