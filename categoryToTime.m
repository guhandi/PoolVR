function time = categoryToTime(C)

strList = string(C);
for i = 1:length(strList)
    
    str = strList(i);
    ch = char(str);
    
    h = "";
    m = "";
    s = "";
    d = "";
    semi = 0;
    for j=1:length(ch)
        num = ch(j);
        if (num == ':')
            semi = semi + 1;
            continue;
        end
        
        if (semi == 0)
            h = h + num;
        end
        if (semi == 1)
            m = m + num;
        end
        if (semi == 2)
            s = s + num;
        end
        if (semi == 3)
            d = d + num;
        end
            
        
    end
    
    hr(i) = str2double(h);
    min(i) = str2double(m);
    sec(i) = str2double(s);
    dec(i) = str2double(d);
    time = [hr; min; sec; dec];
    
end




