function [HR, varargout] = ecgfilter(ecg,fs)

[qrs_amp_raw,qrs_i_raw,delay]=pan_tompkin(ecg,fs,0);

N = length(qrs_amp_raw);
for i = 1:N
    varargout{i} = qrs_amp_raw(i);
end
for i = N+1:25
    varargout{i} = 0;
end
for i = 26:25+N
    varargout{i} = qrs_i_raw(i-25);
end
for i = 26+N:50
    varargout{i} = 0;
end
HR = 40;
if(N >8)
    diffRR = diff(qrs_i_raw(N-8:N));
    HR = 60 * fs / mean(diffRR);
end


end