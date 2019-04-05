function [X, W, stress] = triangleMDS(D, dim, threshold, sampled)
% run my MDS with the threshold as the expected number of outliers
% When threshold == 0, an automatic detection is applied.
% sampled is a matrix with triangles that should be sampled.
% X - the embedding returned.
% W - the filtering matrix (1 are for non outliers)
% stress - the weighted stress result

N = size(D, 1);
% 
% if nargin == 3
%     W = triangle_filter(D, threshold); % Use this one.
% else
%     W = triangle_filter(D, threshold, sampled);
% end

options = optimset('Display','iter');
% [X, stress] = mdscale(D, dim, 'Weights', W, 'Criterion', 'metricstress', 'Start', 'random', 'Options', options);
[X, stress] = mdscale(D, dim, 'Criterion', 'metricstress', 'Start', 'random', 'Options', options);
end