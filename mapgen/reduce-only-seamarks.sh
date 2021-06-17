#!/bin/bash
~/osmosis/bin/osmosis \
--read-pbf-fast $1 \
--tf accept-ways leisure=marina "seamark:type"=* \
--tf reject-ways "seamark:type"=ferry_route \
--tf reject-relations \
--used-node \
--tt /mnt/fast/osmmaps/transform.xml \
\
--read-pbf-fast $1 \
--tf accept-nodes leisure=marina "seamark:type"=* \
--tf reject-ways \
--tf reject-relations \
--tt /mnt/fast/osmmaps/transform.xml \
\
--merge \
\
--write-pbf $2
