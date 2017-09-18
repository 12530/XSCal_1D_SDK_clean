% objective function
function obj = ObjFunLSQnonlin(X, runid,Wobs,chainage)
% Description of parameters:
%   X includes all parameters
%   evalCount is the iteration in one model run.
%   runid is assigned to differentiate from previous model run.
%   Wobs is observed water level at specific station.
% NB: Before running this function, please double check the executive file.
% Double check the path of MIKE stuff in the source code.
%

%% make an unique number
% tt = (now - datenum('14092017','ddmmyyyy'))*100000;
% ttstr = num2str(tt,20);
% evalCount = str2num(ttstr(end-8:end));% make sure no zero starting 
% evalCount = num2str(evalCount);
evalCount = num2str(randi([1 100000]) + randi([100000 500000] + floor(rand*1000)));
mkdir(['.\AutomaticXSCal\Model\run', num2str(runid),'\',evalCount]);
%% copy forcing data, boundary data
[status, message, messageId] = copyfile('.\AutomaticXSCal\Model\HarbinQ2007-14.dfs0',strcat('.\AutomaticXSCal\Model\run', num2str(runid),'\',evalCount,'\HarbinQ2007-14.dfs0'));
[status, message, messageId] = copyfile('.\AutomaticXSCal\Model\JiamusiH2007-14.dfs0',strcat('.\AutomaticXSCal\Model\run', num2str(runid),'\',evalCount,'\JiamusiH2007-14.dfs0'));
[status, message, messageId] = copyfile('.\AutomaticXSCal\Model\Q4tributary2007-14.dfs0',strcat('.\AutomaticXSCal\Model\run', num2str(runid),'\',evalCount,'\Q4tributary2007-14.dfs0'));
[status, message, messageId] = copyfile('.\AutomaticXSCal\Model\Songhua_xsection.xns11',strcat('.\AutomaticXSCal\Model\run', num2str(runid),'\',evalCount,'\Songhua_xsection.xns11'));
[status, message, messageId] = copyfile('.\AutomaticXSCal\Model\SonghuaHDv2.mhydro',strcat('.\AutomaticXSCal\Model\run', num2str(runid),'\',evalCount,'\SonghuaHDv2.mhydro'));
% parameter scaling
datum     = X(1:45);         % datum, i.e.Z0
b         = X(46:90);        % bottom width
tan_alpha = 10.^X(91:135);   % tangent(alpha)
n         = 10.^X(136:180);  % Manning's roughness coefficient
% if ((datumFlag == 45) & (WFlag == 45) & (bFlag == 45) & (nFlag == 1))
%% Step 1 generate the parameter and prepare the updated Songhua_xsection_upd.mat
% .mat should be in the dimension of Nxn (row) and Npar (column), i.e.45*4
% format [chainage, datum, b, tan_alpha]

chaindat_new = [chainage, datum, b, tan_alpha];
matFile = strcat('.\AutomaticXSCal\Scripts\run',num2str(runid),'\Songhua_xsection_upd', evalCount, '.mat');
save(matFile,'chaindat_new');

%% Step 2 update .xns11 file by call XSCal_1D_SDK.exe
%     Usage:
%       XSCal_1D_SDK.exe update_all evalCount runid (3 parameters)
cmd_run_xs = ['XSCal_1D_SDK.exe',' ','update_all',' ',evalCount,' ', num2str(runid)];
disp(cmd_run_xs)
[status output] = system(cmd_run_xs, '-echo');
if status ~= 0
    error(['Error in updating xns11 file (Step 2 in ObjFunLSQnonlin.m). Status: ',num2str(status),'    Output: ',output])
end

%% Step 3 Modify .mhydro file
filename = ['.\AutomaticXSCal\Model\run', num2str(runid),'\',evalCount,'\SonghuaHDv2.mhydro']; % assign filename and path
fin = fopen(filename,'r');
if fin == -1
    error(['Error in modifying .mhydro file. Please check filename: ',filename'])
end
% create a temperal file
fileTemp = ['.\AutomaticXSCal\Model\run',num2str(runid),'\',evalCount,'\SonghuaHDv2-',evalCount,'.mhydro'];
fout = fopen(fileTemp,'w');
check = 0;
flag  = 0;
findN = 0;
% two filenames should be changed, i.e. Line 10 ResultFileArray and Line
% 1212 CrossSectionsFile. And ResistanceNumber is also changed for 45 cs.
while ~feof(fin)
    s = fgets(fin);
    % assign new cross section file
    old_xs = ['Songhua_xsection.xns11'];
    new_xs = ['Songhua_xsection',evalCount,'.xns11'];
    if regexp(s,old_xs)
        s = regexprep(s, old_xs, new_xs);
        check = 1;
    end
    
    % assign new result file
    old_res = ['SonghuaHDv2'];
    new_res = ['SonghuaHDv2-',evalCount];
    if regexp(s,old_res)
        s = regexprep(s, old_res,new_res);
        flag = 1;
    end
    
    % change resistance number
    %         k = strfind(s,'GlobalResistanceNumber');
    k = strfind(s,'            ResistanceNumber =');
    if(k > 0)
        findN = findN + 1;
        start = strfind(s,'=');
        s = [s(1:start+1),num2str(n(findN)), char(10)]; % maintain the format (space = space)
        
    end
    fprintf(fout,'%s',s);
end

if check == 0
    error(['Error in updating cross section file (Step 3)'])
end

if flag == 0
    error(['Error in updating output file (Step 3)'])
end

if findN ~= 45
    error(['Error in updating Manning roughness coefficient (Step 3)'])
end
fclose all;
%% Step 4 run MIKE HYDRO River
disp(['Step 4 run MIKE HYDRO River ', evalCount]);
mikepath = 'C:\PROGRA~2\DHI\2017\bin\x64\DHI.Mike1D.Application.exe'; % use PROGRA~2 instead of Program File (x86)
if ~exist(fileTemp,'file')
    error(['Error in MIKE_HYDRO model run. Specified file ',fileTemp,' could not be found.'])
end

cmd_run_1D = [mikepath,' "',fileTemp,'" -close -maxnumthreads=1']; %added quotation marks to allow spaces in path
[status, output] = system(cmd_run_1D, '-echo'); %-echo prints out the command line to the MATLAB command window
if status ~= 0
    error(['Error in MIKE_HYDRO 1D model run (Step 4 in ObjFunLSQnonlin.m). Status: ',num2str(status),'    Output: ',output])
end
disp(['Step 4 finished ', evalCount])
%% Step 5 read water level or discharge from .res1d file
cmd_run_res1d = ['res1d2mat.exe',' ',num2str(evalCount),' ', num2str(runid)]; % please go to res1d2dat project to check the path of .res1d file
[status output] = system(cmd_run_res1d, '-echo');
if status ~= 0
    error(['Error in reading res1d output (Step 5 in ObjFunLSQnonlin.m). Status: ',num2str(status),'    Output: ',output])
end
% load water level time series from .mat file
load(['.\AutomaticXSCal\Model\run',num2str(runid),'\',evalCount,'\WaterLevelTS',num2str(evalCount),'.mat']); % data are stored in WLsim variable

%% Step 6 define the objective function
Wsim = WLsim(:,23); % assign the correct column number for corresponding cross section
% assign the regularization parameters
w1 = 0.1;
w2 = 0.1;
w3 = 0.1;
% ---------- deal with the offset (bias of two datum) ------------------- ?
rms = @(dh)sqrt(mean((Wsim + dh - Wobs).^2));
bias = fminsearch(rms,0);
Wsim = Wsim + bias;
% Chainage of Tonghe: 230000 corresponding to cross section #23 at 228573 m
% Chainage of Yilan: 331500 corresponding to cross section #33 at 327953 m
obj = sum((Wobs - Wsim).^2) + w1*sum((b(1:end-1) - b(2:end)).^2) + ...
    w2*sum((tan_alpha(1:end-1) - tan_alpha(2:end)).^2) + w3*sum((n(1:end-1) - n(2:end)).^2);

%% Step 7 clean old folder
[status, message, messageid] = rmdir(['.\AutomaticXSCal\Model\run',num2str(runid),'\',evalCount], 's');
end