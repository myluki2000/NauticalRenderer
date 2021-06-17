#!/bin/bash
~/osmosis/bin/osmosis \
\
--read-pbf-fast $1 \
--tf accept-ways place=city,village,town \
--used-node \
--tf reject-relations \
--tt /mnt/fast/osmmaps/transform.xml \
\
--read-pbf-fast $1 \
--tf accept-nodes place=city,village,town \
--tf reject-ways \
--tf reject-relations \
--tt /mnt/fast/osmmaps/transform.xml \
\
--read-pbf-fast $1 \
--tf accept-relations place=city,village,town \
--used-way \
--used-node \
--tt /mnt/fast/osmmaps/transform.xml \
\
--merge \
--merge \
\
--write-pbf $2
