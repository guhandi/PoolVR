%direction
%variability in blocks of 25
%contat point of cue
%angle of cue at contact

%%Function to process the Unity Text data
%return UnityData = {time, x position, z position, x velocity, z velocity}
%param: data, unity pocket x position vector (for scaling)
function UnityData = Acquire_and_Preprocess(data)

%Unity variables to return
timedata={}; xdata={}; zdata={}; xdot={}; zdot={};

% load data
trial = grp2idx(table2array(data(:,1)));
time = table2array(data(:,2));
csPos = table2array(data(:,3));
cbPos = table2array(data(:,6));
cbVel = table2array(data(:,7));

%get position and velocity vectors
cuestickpos = categoryToVector(csPos);
pos = categoryToVector(cbPos);
vel = categoryToVector(cbVel);

%scale factor us to make wifht of table 0.5
pocketx = [-0.0495, 0.6466,-0.0495, 0.6466,-0.0495, 0.6466,];
w = abs(pocketx(1) - pocketx(2));
us = 1/w;

%intial position of cueball
xstart = pos(1,1);
ystart = pos(1,2);
zstart = pos(1,3);

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

%go through data for each trial
tr=0; %good trial number
for t=1:trial(end)-1

    %Get trial specific index
    trialnum = t;
    idx = find(trial == trialnum);
    trtime = time(idx);
    if (trtime(end) < 1) %if trial time less than 1 second then consider "bad" trial
        continue;
    end
    tr=tr+1; %increment good trial for indexing

    x = us * (pos(idx,1)-xstart); %x position data for specific trial
    y = us * (pos(idx,2)-ystart); %y position data for specific trial
    z = us * (pos(idx,3)-zstart); %z position data for specific trial
    xdot = vel(idx,1);
    ydot = vel(idx,2);
    zdot = vel(idx,3);

    %delete all data before shot hit (t=0 is hit)
    ishit = find(abs(zdot) > 0.001);
    if (length(idx) == 0)
        ishit(1) = 1; %missed ball or glitch?
    end
    start = ishit(1);
    trtime = trtime(start:end);
    x=x(start:end); y=y(start:end); z=z(start:end);
    xdot=xdot(start:end); ydot=ydot(start:end); zdot=zdot(start:end);
    
    
    
    %Direction = mean angle during first 20 frames of hit (about 0.25 sec)
    [maxz, zid] = max(zdot);
    meanx = mean(x(zid:zid+20)); meanz = mean(z(zid:zid+20))
    rad = atan( meanz / meanx );
    if (rad < 0) rad = rad + pi; end
    deg = rad / pi * 180;
    
    
    %Store Data
    direction(tr) = deg;
    cshitpos{tr} = cuestickpos(start,:);
    posData{tr} = [x y z];
    velData{tr} = [xdot ydot zdot];
    timedata{tr} = trtime;

end


UnityData.time = timedata; UnityData.pos = posData; UnityData.vel = velData;
UnityData.cspos = cshitpos;
UnityData.direction = direction;





%%Helper function to conver category data to points
function [v] = categoryToVector(C)

strList = string(C);
for i = 1:length(strList)
    
    str = strList(i);
    ch = char(str);
    
    xpos = "";
    ypos = "";
    zpos = "";
    semi = 0;
    for j=1:length(ch)
        num = ch(j);
        if (num == ';')
            semi = semi + 1;
            continue;
        end
        
        if (semi == 0)
            xpos = xpos + num;
        end
        if (semi == 1)
            ypos = ypos + num;
        end
        if (semi == 2)
            zpos = zpos + num;
        end
        
    end
    
    x(i) = str2double(xpos);
    y(i) = str2double(ypos);
    z(i) = str2double(zpos);
    v=[x; y; z]';
    
end
