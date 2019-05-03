function CameraData = camera(data, datatrial)

CameraData = {};

% load position data
%data = PILOTGS9042420191928position;
col1 = table2array(data(:,1));
col2 = table2array(data(:,2));
col3 = table2array(data(:,3));
col4 = table2array(data(:,4));
col5 = table2array(data(:,5));
col6 = table2array(data(:,6));

%load trial data
%datatrial = PILOTGS9042420191928beginTrial;
tnum = table2array(datatrial(:,3));
numtrials = length(tnum) - 1;
window = 1000;

%start data
xo = 247; zo = 687;
%xo = 262; zo = 701;
corner1x = 450; corner1z = 800;
w = 400; us = 1/w;

cbx = {}; cbz = {}; rbx = {}; rbz = {};
timedata = {}; timesec = {};
xdata = {}; zdata = {};
for i=1:numtrials
    
    start = tnum(i);
    ed = tnum(i+1);
    
    idx = find(col1 == start);
    ide = find(col1 == ed);
    
    time = categoryToTime(col2(idx : ide));
    cbzpos = col3(idx : ide)';
    cbzd{i} = cbzpos;
    cbxpos = col4(idx : ide)';
    cbxd{i} = cbxpos;
    rbxpos = col5(idx : ide)';
    rbzpos = col6(idx : ide)';
    
    lost = find(cbxpos ~= 35);
    %lostz = find(cbzpos ~= 135);
    t = time(:,lost);
    x = cbxpos(lost);
    z = cbzpos(lost);
    xd= -us * (x-xo); %scale and shift
    zd= -us * (z-zo); %scale and shift
    
    %fix losing track (sporadic changes)
    bad = find(xd > 0.45);
    xd(bad) = [];
    zd(bad) = [];
    t(:,bad) = [];
       
    %set variables for each trial
    timedata(i) = {t};
    cbx{i} = xd;
    cbz{i} = zd;
    

end

timesec = getTime(timedata);
for t=1:length(cbz)
    idx = find(cbz{t} > 0.01);
    if (length(idx) == 0)
       start = 1; 
    else
        start = idx(1);
    end
    timesec{t} = timesec{t} - timesec{t}(start);
    
end

CameraData = {timesec; cbx; cbz};



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


