#!/bin/bash
~/osmosis/bin/osmosis \
--read-pbf-fast $1 \
--tf accept-ways leisure=marina natural=coastline water=* man_made=pier,breakwater "seamark:type"=* wetland=tidalflat \
--tf reject-ways water=pond,stream,ditch,fishpond,stream_pool,basin,reservoir,fish_pass,reflecting_pool,moat,wastewater "seamark:type"=ferry_route \
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
--read-pbf-fast $1 \
--tf accept-relations water=* wetland=tidalflat admin_level=2 \
--tf reject-relations water=pond,stream,ditch,fishpond,stream_pool,basin,reservoir,fish_pass,reflecting_pool,moat,wastewater "seamark:type"=ferry_route \
--used-way \
--used-node \
--tt /mnt/fast/osmmaps/transform.xml \
\
--merge \
--merge \
\
--write-pbf $2