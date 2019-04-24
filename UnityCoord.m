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
w = abs(pocketx(1) - pocketz(1));
us = 0.5/w;
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

x = cbXpos(idx); %x position data for specific trial
y = cbYpos(idx);
z = cbZpos(idx);

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

s=1;
d=25;
sidx = find(zdot > 0.0001);
if (length(sidx) == 0)
    continue;
end
start = sidx(1) + s;
fin = start + d;
xwindow = xdot(start:fin);
zwindow = zdot(start:fin);
theta_init = theta(start:fin);
thetad_init = thetad(start:fin);

trajectory(t) = mean(thetad_init);

end

xstart = pocketx(1);
zstart = pocketz(1);
pocketx = pocketx - xstart;
pocketz = pocketz - zstart;
figure
hold on
plot(pocketx, pocketz, 'o','linewidth',2);

for j=1:5
x=xdata{j};
z=zdata{j};
x=x-xstart;
z=z-zstart;

plot(x,z, 'LineWidth', 2)
xlabel('x');
ylabel('x')
end

end
