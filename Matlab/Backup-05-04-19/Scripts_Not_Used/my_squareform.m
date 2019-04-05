function Z = my_squareform(Y)
%SQUAREFORM Reformat a distance matrix between upper triangular and square form.
%   Z = SQUAREFORM(Y), if Y is a vector as created by the PDIST function,
%   converts Y into a symmetric, square format, so that Z(i,j) denotes the
%   distance between the i and j objects in the original data.
%
%   Y = SQUAREFORM(Z), if Z is a symmetric, square matrix with zeros along
%   the diagonal, creates a vector Y containing the Z elements below the
%   diagonal.  Y has the same format as the output from the PDIST function.
%
%   Z = SQUAREFORM(Y,'tovector') forces SQUAREFORM to treat Y as a vector.
%   Y = SQUAREFORM(Z,'tomatrix') forces SQUAREFORM to treat Z as a matrix.
%   These formats are useful if the input has a single element, so it is
%   ambiguous as to whether it is a vector or square matrix.
%
%   Example:  If Y = (1:6) and X = [0  1  2  3
%                                   1  0  4  5
%                                   2  4  0  6
%                                   3  5  6  0],
%             then squareform(Y) is X, and squareform(X) is Y.
%
%   See also PDIST.

%   Copyright 1993-2011 The MathWorks, Inc.

[m, n] = size(Y);


if m~=n || ~all(diag(Y)==0)
    error(message('stats:squareform:BadInputMatrix'));
end
Z = Y(tril(true(n),-1));
Z = full(Z(:)');                 % force to a row vector, even if empty
end