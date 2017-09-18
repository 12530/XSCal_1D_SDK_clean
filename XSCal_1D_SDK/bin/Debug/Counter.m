function count = Counter()
global runid;
runid = 1;
% f = fopen('Counter.txt', 'a');
% fprintf(f, '1\n');
% fclose(f);
if ~exist('no_par','var')
    no_par = 1;
end
% load('.\AutomaticXSCal\Scripts\runid.mat')
load(['.\AutomaticXSCal\Scripts\run',num2str(runid),'\counter.mat'])
load(['.\AutomaticXSCal\Scripts\run',num2str(runid),'\defaults.mat'],'no_par');

%introduce a break dependent on the current call to
%avoid concurrent load/save of the counter .mat file
pause((counter/no_par - floor(counter/no_par)+0.1)/10)
count = counter;
counter = counter + 1;
save(['.\AutomaticXSCal\Scripts\run',num2str(runid),'\counter.mat'],'counter')
end