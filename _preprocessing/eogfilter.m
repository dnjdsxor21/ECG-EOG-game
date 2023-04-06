function [N, varargout] = eogfilter(eog,fs)

eog = eog(:);
buffsize = length(eog);
%% lowpass filter fc = 10
Wn = 10*2/fs;
N = 3;                   
[a,b] = butter(N,Wn,'low');    
eog_l = filtfilt(a,b,ecg); 
eog_l = eog_l/ max(abs(eog_l));

%% highpass filter fc = 0.5
Wn = 0.5*2/fs;
N = 3;               
[c,d] = butter(N,Wn,'high'); 
eog_h = filtfilt(c,d,eog_l); 
eog_h = eog_h/ max(abs(eog_h));

%% out
% flag = 0 입력대기
% flag = 1 입력
% flag = 2 입력금지

for i = 1:buffsize
    varargout{i} = eog_h(i);
end
        




end