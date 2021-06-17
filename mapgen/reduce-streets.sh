#!/bin/bash
~/osmosis/bin/osmosis \
\
--read-pbf-fast $1 \
--tf accept-ways highway=* railway=* \
--tf reject-ways highway=raceway,bus_guideway,bridleway,construction railway=abandoned,construction,disused,miniature \
--used-node \
--tf reject-relations \
--tt /mnt/fast/osmmaps/transform.xml \
\
--write-pbf $2
