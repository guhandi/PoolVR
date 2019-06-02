%% Function to process the Camera Text data
%return CameraData = {time, x position, z position}
%param: position data, trial data
function CameraData = camera(data, datatrial)

CameraData = {};

% load position data
col1 = table2array(data(:,1)); %frame
col2 = table2array(data(:,2)); %time
col3 = table2array(data(:,3)); %cueball x
col4 = table2array(data(:,4)); %cueball z
col5 = table2array(data(:,5)); %redball x
col6 = table2array(data(:,6)); %redball z

%load trial data
tnum = table2array(datatrial(:,3)); %trial numbers
numtrials = length(tnum) - 1;

%start data
xo = 247; zo = 687;
%xo = 251; zo = 699;
w = 349; us = 1/w; %w is width

cbx = {}; cbz = {};
timedata = {}; 
for i=1:numtrials-1
    
    startframe = tnum(i);
    endframe = tnum(i+1);
    idx = find(col1 == startframe); %start row in data for specific trial number
    ide = find(col1 == endframe); %end row in data for specific trial number
    
    time = categoryToTime(col2(idx : ide));
    cbzpos = col3(idx : ide)';
    cbxpos = col4(idx : ide)';
    %rbxpos = col5(idx : ide)';
    %rbzpos = col6(idx : ide)';
    
    lostx = find(cbxpos ~= 35);
    %lostz = find(cbzpos ~= 135);
    t = time(:,lostx);
    x = cbxpos(lostx);
    z = cbzpos(lostx);
    xd= -us * (x-xo); %scale and shift so xd = [0, 0.5]
    zd= -us * (z-zo); %scale and shift so zd = [0, 1]
    
    
    %fix sporadic changes in camera due to interference
    err=0.25;
    for b=length(zd):-1:2
        if( abs( zd(b) - zd(b-1)) > err)
            xd(b)=[];
            zd(b)=[];
            t(:,b)=[];
        end
    end
           
    %set variables for each trial
    timedata(i) = {t};
    cbx{i} = xd;
    cbz{i} = zd;
    cbxdot{i} = diff(xd);
    cbzdot{i} = diff(zd);
    

end

%shift time so cue stick - cue ball collision is at t=0
timesec = getTime(timedata);
for t=1:length(cbzdot)
    
    idx = find(cbz{t} > 0.05); %find index where z position changes
    if (isempty(idx))
       start = 1; 
    else
        start = idx(1); 
    end
    timesec{t} = timesec{t} - timesec{t}(start);
    
end

CameraData = {timesec; cbx; cbz};


%function to convert the category time data form the camera to a continuous
%time data
function second = getTime(t)
    second = {};
    for tr = 1:length(t)
        timec = t{tr};
        
        for dim=1:size(timec,2)
            tsec(dim) = 3600 * timec(1,dim) + 60 * timec(2,dim) + timec(3,dim) + 0.001 * timec(4,dim);
        end
        second{tr} = tsec - tsec(1);
        tsec = [];

    end   
end

end


