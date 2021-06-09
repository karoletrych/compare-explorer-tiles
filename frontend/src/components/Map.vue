<template>
  <l-map :zoom="zoom" :center="center" @update:bounds="setBounds">
    <l-tile-layer :url="url" :attribution="attribution" />

    <l-polyline
      :lat-lngs="gridLines.latlngs"
      :color="gridLines.color"
      :weight="0.5"
    />
    <l-polyline
      v-for="user in users"
      :key="user.name"
      :lat-lngs="user.routes"
      :color="user.color"
      :weight="1"
    />
    <l-polyline
      v-for="user in users"
      :key="user.name"
      :lat-lngs="user.tiles"
      :color="user.color"
      :fillColor="user.color"
      fill
      :weight="0.5"
    />
  </l-map>
</template>

<script lang="ts">
import Vue from "vue";
import { LatLngBounds } from "leaflet";
import { tile2lat, tile2long, calcTile } from "@/Utils";
import { LTileLayer, LPolyline, LMap } from "vue2-leaflet";


export default Vue.extend({
  name: "Map",
  props: {
      users: {type: Object}
  },
  data() {
    return {
      url: "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
      attribution:
        '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors',
      zoom: 11,
      center: [50.079147, 19.9497],
      gridLines: {},
      bounds: {} as LatLngBounds
    };
  },
  components: { LMap, LTileLayer, LPolyline },
  methods: {
    addGridLines() {
      let gridLines = [];
      let n = this.bounds.getNorthWest();
      let i = this.bounds.getSouthEast();
      let nortthWestTile = calcTile(n.lat, n.lng);
      let southEastTile = calcTile(i.lat, i.lng);

      // vertical lines
      for (let s = nortthWestTile.x - 5; s <= southEastTile.x + 5; s++) {
        gridLines.push([
          [tile2lat(nortthWestTile.y - 5), tile2long(s)],
          [tile2lat(southEastTile.y + 5), tile2long(s)]
        ]);
      }

      // horizontal lines
      for (let l = nortthWestTile.y - 5; l <= southEastTile.y + 5; l++)
        gridLines.push([
          [tile2lat(l), tile2long(nortthWestTile.x - 5)],
          [tile2lat(l), tile2long(southEastTile.x + 5)]
        ]);
      this.gridLines = {
        latlngs: gridLines,
        color: "gray",
        weight: 0.1,
        opacity: 0.2
      };
    },
    setBounds(e: any) {
      this.bounds = e;
      this.addGridLines();
    }
  }
});
</script>

<style></style>
