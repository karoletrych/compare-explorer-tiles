<template>
  <div id="app">
    <div class="topmenu">
        <a :href="authUri">login</a>
      <export :content="{ tiles: myTiles, routes: myRoutes }" />
      <import @file-imported="fileImported" />
    </div>
    <user-list v-model="users" />
    <l-map
      :zoom="zoom"
      :center="center"
      @update:bounds="setBounds"
      style="height: 85%; width: 100%"
    >
      <l-tile-layer :url="url" :attribution="attribution" />

      <l-polyline
        :lat-lngs="gridLines.latlngs"
        :color="gridLines.color"
        :weight="0.5"
      />
      <l-polyline
        v-for="user in visibleUsers"
        :key="user.name"
        :lat-lngs="user.routes"
        :color="user.color"
        :weight="1"
      />
      <l-polyline
        v-for="user in visibleUsers"
        :key="user.name"
        :lat-lngs="user.tiles"
        :color="user.color"
        :fillColor="user.color"
        fill
        :weight="0.5"
      />
    </l-map>
  </div>
</template>

<script lang="ts">
import Vue from "vue";

import { LMap, LTileLayer, LPolyline } from "vue2-leaflet";
import { LatLngBounds } from "leaflet";
import "leaflet/dist/leaflet.css";
import UserList from "./components/UserList.vue";
import Import from "./components/Import.vue";
import Export from "./components/Export.vue";

const DIV = Math.pow(2, 14);

Vue.component("l-map", LMap);

let clientId = 62340;
let redirectUri = process.env.VUE_APP_API_URI + "Auth";

let authUri =
  `http://www.strava.com/oauth/authorize?` +
  `client_id=${clientId}&` +
  `response_type=code&` +
  `redirect_uri=${redirectUri}&` +
  `approval_prompt=force&` +
  `scope=read,activity:read`;



interface Tile {
  x: number;
  y: number;
}
interface User {
  show: boolean;
  color: string;
  name: string;
  total: number;
  tiles: any;
  routes: any;
  activities: number;
}

function tile2long(t: number) {
  return (t / DIV) * 360 - 180;
}
function tile2lat(t: number) {
  let n = Math.PI - (2 * Math.PI * t) / DIV;
  return (180 / Math.PI) * Math.atan(0.5 * (Math.exp(n) - Math.exp(-n)));
}

export default Vue.extend({
  name: "App",
  components: {
    LMap,
    LTileLayer,
    LPolyline,
    UserList,
    Import,
    Export,
  },
  async mounted() {
    let response = await fetch(
      process.env.VUE_APP_API_URI + "GetActivities", 
      {credentials: "include"} // cross origin cookie
    ).then((x) => x.json());
    this.myRoutes = response.routes;
    this.myTiles = this.tilesToLatLng(response.tiles);
    this.users.push({
      name: "me",
      show: true,
      color: "#ff0000",
      total: response.tiles.length,
      activities: response.routes.length,
      routes: this.myRoutes,
      tiles: this.myTiles,
    });
  },
  computed: {
    visibleUsers() : User[] {
      return this.users.filter((x) => x.show);
    },
  },
  data() {
    return {
      zoom: 11,
      center: [50.079147, 19.9497],
      bounds: {} as LatLngBounds,
      gridLines: {},
      myRoutes: [],
      myTiles: [] as number[][][],
      url: "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
      attribution:
        '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors',
      route: null,
      users: [] as User[],
      authUri: authUri
    };
  },
  methods: {
    setBounds(e: any) {
      this.bounds = e;
      this.addGridLines();
    },
    addGridLines() {
      let gridLines = this.getGridLines();
      this.gridLines = {
        latlngs: gridLines,
        color: "gray",
        weight: 0.1,
        opacity: 0.2,
      };
    },
    getGridLines() {
      let gridLines = [];
      let n = this.bounds.getNorthWest();
      let i = this.bounds.getSouthEast();
      let nortthWestTile = this.calcTile(n.lat, n.lng);
      let southEastTile = this.calcTile(i.lat, i.lng);

      // vertical lines
      for (let s = nortthWestTile.x - 5; s <= southEastTile.x + 5; s++) {
        gridLines.push([
          [tile2lat(nortthWestTile.y - 5), tile2long(s)],
          [tile2lat(southEastTile.y + 5), tile2long(s)],
        ]);
      }

      // horizontal lines
      for (let l = nortthWestTile.y - 5; l <= southEastTile.y + 5; l++)
        gridLines.push([
          [tile2lat(l), tile2long(nortthWestTile.x - 5)],
          [tile2lat(l), tile2long(southEastTile.x + 5)],
        ]);

      return gridLines;
    },

    calcTile(lat: number, lng: number) {
      return {
        x: Math.floor(((lng + 180) / 360) * DIV),
        y: Math.floor(
          ((1 -
            Math.log(
              Math.tan(this.deg2rad(lat)) + 1 / Math.cos(this.deg2rad(lat))
            ) /
              Math.PI) /
            2) *
            DIV
        ),
      };
    },
    deg2rad(t: number) {
      return t * (Math.PI / 180);
    },
    tilesToLatLng(tiles: Tile[]) {
      let tileCoords = tiles.map((tile) => {
        let a = tile2lat(tile.y);
        let b = tile2long(tile.x);

        let c = tile2lat(tile.y + 1);
        let d = tile2long(tile.x);

        let e = tile2lat(tile.y + 1);
        let f = tile2long(tile.x + 1);

        let g = tile2lat(tile.y);
        let h = tile2long(tile.x + 1);

        let i = tile2lat(tile.y);
        let j = tile2long(tile.x);

        return [
          [a, b],
          [c, d],
          [e, f],
          [g, h],
          [i, j],
        ];
      });
      return tileCoords;
    },
    fileImported(json: Record<string, any>) {
      let name = prompt("name?") || "";
      this.users.push({
        tiles: json.tiles,
        routes: json.routes,
        total: json.tiles.length,
        activities: json.routes.length,
        show: true,
        color: "#0000ff",
        name: name,
      });
    },
  },
});

// function userTiles2tilesToUsers(userRoutePoints){
//   let map = {}
//   for(let user in userRoutePoints){
//     userRoutePoints[user].forEach(p => {
//       if(!map[p]){
//         map[p] = [user]
//       }
//       else{
//         map[p].push(user)
//       }
//     })
//   }
//   return map;
// }
</script>

<style>
.topmenu {
  position: fixed;
  width: 100%;
  height: 58px;
  top: 0;
  left: 0;
}
html, 
body,
#app {
    height: 100%;
}

#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>
