function Q = quatlog(Q0)
% quatlog
%
% Normal logarithm of quaternions
%
% -------------------------------------------------------------------------
% Author: 
% -------------------------------------------------------------------------
% Andreas Aristidou (a.aristidou@ieee.org)
%     - Created: 2016.06.22
% - Last Edited: 2016.06.22
% -------------------------------------------------------------------------

warning off

s = Q0(1);
v = Q0(2:4);

Q(1) = log(norm(Q0));

Q(2:4) = v/norm(v).*acos(s/norm(Q0));

if Q0(1) == 1 && Q0(2) == 0 && Q0(3) == 0 && Q0(4) == 0
    Q = [0 0 0 0];
end