function Y_full = MDS_Matlab
D = [
    0   569  667  530  141  ;
    569 0    1212 1043 617  ;
    667 1212 0    201  596; ;
    530 1043 201  0    431  ; 
    141 617  596  431  0     ];  %This is the Eucildean distance matrix of these five cities
fprintf('*************************************************************\n');
fprintf(' Load Initial_data    \n the Euclidean distance matrix D is')
D
fprintf('Program paused. Press enter to continue.\n');
fprintf('*************************************************************\n');
pause;
%--------------------------------------------------------------------------
D2 = D.*D;
fprintf(' the  matrix  D2  is')
D2
fprintf('Program paused. Press enter to continue.\n');
fprintf('*************************************************************\n');
pause;
%--------------------------------------------------------------------------
M = 2;          % Two dimensions
len = size(D2, 1);
J = eye(len) - 1 / len * ones(len);
B = -1/2 * J * D2 * J;
fprintf(' The  matrix  J and B  is')
J
B
fprintf('Program paused. Press enter to continue.\n');
fprintf('*************************************************************\n');
pause;
%--------------------------------------------------------------------------
[eigvec,eigval] = eig(B);
fprintf(' The  Eigenvector and Eigenvalue of B is\n')
EigenVector=eigvec
EigenValue=eigval
fprintf('Program paused. Press enter to continue.\n');
fprintf('*************************************************************\n');
pause;
%--------------------------------------------------------------------------
[eigval, order] = sort(max(eigval)','descend');
eigvec = eigvec(order,:);
eigvec = eigvec(:,1:M); 
eigval = eigval(1:M);
fprintf(' the M number of largest Eigenvalue and it Eigenvector is')
EigenVector=eigvec
EigenValue=eigval'
fprintf('Program paused. Press enter to continue.\n');
fprintf('*************************************************************\n');
pause;
%--------------------------------------------------------------------------
fprintf('The Coordinate map of these five cities\n')
temp1=[eigval(1),0;0,eigval(2)];
temp1=(temp1).^(1/2);
temp=eigvec*temp1;
x0=temp(:,1);
y0=temp(:,2);
c1=x0;
c2=y0;
figure,plot(x0,y0,'r*');
hold on 
plot(c1,c2,'ro');
set(gca,'ytick',[-100:25:150])
set(gca,'xtick',[-800:50:600])
grid on


Y_full = zeros(5,2);
for ii=1:length(x0)
        text(x0(ii)+0.2,y0(ii)+0.2,['   (',num2str(x0(ii)),' , ',num2str(y0(ii)),')']);
        Y_full(ii,1)=x0(ii);
        Y_full(ii,2)=y0(ii);
end
Y_full
title('The Coordinate map of these five cities')
fprintf('Program paused. Press enter to continue.\n');
fprintf('*************************************************************\n');
pause;
%--------------------------------------------------------------------------

A=[eigval(1),0;0,eigval(2)];
X = eigvec*sqrt(A);
recompute_matrix = D;
for i = 1:len
    for j = 1:len
        recompute_matrix(i, j) = norm(X(i, :) - X(j, :));
    end
end
fprintf(' the Re-compute distance matrix is')
recompute_matrix
fprintf('Program paused. Press enter to continue.\n');
fprintf('*************************************************************\n');
pause;