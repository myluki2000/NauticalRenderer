#!/bin/bash
~/osmosis/bin/osmosis \
--read-pbf-fast $1 \
--tf accept-ways landuse=commerical,industrial,residential,retail,forest \
--used-node \
--tt transform.xml \
\
--write-pbf $2
