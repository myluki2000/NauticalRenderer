#!/bin/bash
~/osmosis/bin/osmosis \
--read-pbf-fast $1 \
--tf accept-ways leisure=marina natural=coastline,water man_made=pier,breakwater "seamark:type"=* wetland=tidalflat boundary=protected_area \
--tf reject-ways water=pond,stream,ditch,fishpond,stream_pool,basin,reservoir,fish_pass,reflecting_pool,moat,wastewater "seamark:type"=ferry_route \
--tf reject-relations \
--used-node \
--tt transform.xml \
\
--read-pbf-fast $1 \
--tf accept-nodes leisure=marina "seamark:type"=* \
--tf reject-ways \
--tf reject-relations \
--tt transform.xml \
\
--read-pbf-fast $1 \
--tf accept-relations natural=water wetland=tidalflat admin_level=2 boundary=protected_area \
--tf reject-relations water=pond,stream,ditch,fishpond,stream_pool,basin,reservoir,fish_pass,reflecting_pool,moat,wastewater "seamark:type"=ferry_route \
--used-way \
--used-node \
--tt transform.xml \
\
--merge \
--merge \
\
--write-pbf $2
