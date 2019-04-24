function [x,y,z] = categoryToVector(C)

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
    
end




