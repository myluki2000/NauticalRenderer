#!/bin/bash
cd $1
osmium extract \
-p boundary.poly \
--overwrite \
--strategy=complete_ways \
../eu.osm.pbf \
-o "$1.osm.pbf"
