function [xdata,zdata] = camera(data, datatrial)

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
numtrials = length(tnum);
window = 500;


cbx = zeros(numtrials,window);
cbz = zeros(numtrials,window);
rbx = zeros(numtrials,window);
rbz = zeros(numtrials,window);
xdata = {};
zdata = {};
thetadata = {};
for i=1:numtrials
    start = tnum(i);
    idx = find(col1 == start);
    cbxpos = col4(idx : idx+window-1)';
    cbzpos = col3(idx : idx+window-1)';
    rbxpos = col5(idx : idx+window-1)';
    rbzpos = col6(idx : idx+window-1)';
    
    
    cbx(i,:) = cbxpos;
    cbz(i,:) = cbzpos;
    rbx(i,:) = rbxpos;
    rbx(i,:) = rbzpos;
    
    
end

xo = 247;
zo = 687;
corner1x = 450;
corner1z = 800;
w = 400;
us = 1/w;
%get rid of lost values
for j=1:numtrials
    tr = j;
    x = cbx(tr, find(cbxpos ~= 135));
    z = cbz(tr, find(cbzpos ~= 35));
    xd= -us * (x-xo);
    zd= -us * (z-zo);
    
    
    
    theta = atan(z./x);
    xdata{j} = xd;
    zdata{j} = zd;
    thetadata{j} = theta;
    
    %id = find(z>1);
    %zstart = z(id(1)-10 : id(1) + 100);
    %plot(theta(id(1) : id(1) + 100))
    plot(xd,zd,'linewidth',2);
    %plot(zstart,'linewidth',2);
    
  
end

end


