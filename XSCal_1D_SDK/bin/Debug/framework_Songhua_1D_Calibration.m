% calibrate a hydrodynamic model using lsqnonlin with Levenberg-Marquardt
% method. The processes are listed as below:
% 1. load observation and assign run number.
% 2. call lsqnonlin solver, which will call ObjFunLSQnonlin objective
% function.
% 3. In ObjFunLSQnonlin function, new parameter set will be passed and
% evaluation will perform.
% See details in ObjFunLSQnonlin.

clear
clc
close all

% load observed water level
Wobs = load('WL_yilan.txt');
% initial values below are obtained from Songhua_xsection_ini.mat
% load chainage
load('chainage.mat'); % chainage always keep the same.
load('datum0.mat'); % load datum0
% 4*45 parameters 
datum0     = datum0;                % datum
b0         = ones(45,1)*650;        % initial bottom width, 350 m
tan_alpha0 = ones(45,1)*(-1.0);     % log10(tan_alpha) = -1 => tan_alpha = 0.1 => alpha = 5.7 degree
n0         = ones(45,1)*(-1.5);     % log10(n) = -1.5  => n = 0.0316
X0 = [datum0;b0;tan_alpha0;n0];% starting location of LM algorithm 
global runid;
runid = 1; % change this number in case previous result will be override.
no_par = 3;
counter = 1;
% --------------------------------------------------------------------
% check whether the folder exist
modelFolder = strcat('.\AutomaticXSCal\Model\run',num2str(runid));
if ~exist(modelFolder,'dir')
    mkdir(modelFolder);
end
scriptFolder = strcat('.\AutomaticXSCal\Scripts\run',num2str(runid));
if ~exist(scriptFolder,'dir')
    mkdir(scriptFolder);
end
% --------------- Set up shared variables with OUTFUN ----------------
bL         = ones(45,1)*400;    
tan_alphaL = ones(45,1)*0.05;     
nL         = ones(45,1)*(-3);     
LB = [datum0 - 5;bL;tan_alphaL;nL];
bU         = ones(45,1)*900;      
tan_alphaU = ones(45,1)*0.5;      
nU         = ones(45,1)*(-1);     
UB = [datum0 + 5;bU;tan_alphaU;nU];
% ---------------- Call optimization ---------------------------------
options = optimoptions(@lsqnonlin, ...       % optimization method
    'Algorithm','levenberg-marquardt',...    % algorithm
    'DiffMinChange',1e-3,...                 % stop condition
    'MaxIterations',1000,...                 % stop condition
    'OutputFcn',@Outputfunc,...              % output function
    'UseParallel',true);                     % parallel
tStart = tic;

parpool(no_par)
% options = gaoptimset('InitialPopulation',X0','PopulationSize',200,'Generations',5000,'MutationFcn',@mutationadaptfeasible,'UseParallel',true);
% [xopt, fval,exitflag,output, population,score] = ga(@(X)ObjFunLSQnonlin(X,runid,Wobs,chainage),180,[],[],[],[],LB',UB',[],[],options);
[x,resnorm,residual,exitflag,output,lambda,jacobian] = lsqnonlin(@(X)ObjFunLSQnonlin(X,runid,Wobs,chainage),X0,[],[],options);
delete(gcp('nocreate'))
CI = nlparci(x,residual,jacobian);
tEnd = toc(tStart);
HH = floor(tEnd/3600);
MM = floor((tEnd - HH*3600)/60);
SS = rem(tEnd,60);
fprintf('Time used: %d hours , %d minutes and %f seconds\n',HH, MM, SS)
%% ---------------- Output function ----------------------------
function stop = Outputfunc(x,optimValues,state)
switch state
    case 'iter'
        % Make updates to plot or guis as needed
        %         matFileFolder = strcat('.\AutomaticXSCal\Scripts\run',num2str(runid));
        %         matFile = strcat(matFileFolder,'\optimValues', num2str(evalCount), '.mat');
        %         if ~exist(matFileFolder,'dir')
        %             mkdir(matFileFolder);
        %         end
        %         save(matFile,'optimValues');
% %         history.fval = [history.fval; optimValues.fval];
% %         history.x = [history.x; x];
% %         searchdir = [searchdir; optimValues.searchdirection'];
    case 'interrupt'
        % Probably no action here. Check conditions to see
        % whether optimization should quit.
        
    case 'init'
        % Setup for plots or guis
    case 'done'
        % Cleanup of plots, guis, or final plot
    otherwise
end
stop = false;
end
% end output function
%% performance metrics
% load(['.\AutomaticXSCal\Model\run',num2str(runid),'\WaterLevelTS7091.mat'])% load the result of last run
% Wsim = WLsim(:,23);
% % bias correction
% rms = @(dh)sqrt(mean((Wsim + dh - Wobs).^2));
% bias = fminsearch(rms,0);
% Wsim = Wsim + bias;
% 
% RMSE = sqrt(mean((Wsim - Wobs).^2));
% SSR  = sum((Wsim - Wobs).^2);
% NS   = 1 - (SSR./(sum((Wobs - mean(Wobs)).^2)));
% PBIAS = 100*(sum(Wsim) - sum(Wobs))./sum(Wobs);
% R2 = power((sum((Wobs - mean(Wobs)).*(Wsim - mean(Wsim))))/sqrt(sum(power(Wobs - mean(Wobs),2)).*sum(power(Wsim - mean(Wsim),2))),2);
