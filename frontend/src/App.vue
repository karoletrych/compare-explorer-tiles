<template>
  <div id="app">
    <div class="topmenu">
      <a :href="authUri">login</a>
      <export :content="{ tiles: myTiles, routes: myRoutes }" />
      <import @file-imported="fileImported" />
    </div>
    <user-list v-model="users" />
    <Map :users="visibleUsers" style="height: 85%; width: 100%"> </Map>
  </div>
</template>

<script lang="ts">
import Vue from "vue";

import "leaflet/dist/leaflet.css";
import UserList from "./components/UserList.vue";
import Import from "./components/Import.vue";
import Export from "./components/Export.vue";
import Map from "./components/Map.vue";
import { tilesToLatLng } from "./Utils";


let clientId = 62340;
let redirectUri = process.env.VUE_APP_API_URI + "Auth";

let authUri =
  `http://www.strava.com/oauth/authorize?` +
  `client_id=${clientId}&` +
  `response_type=code&` +
  `redirect_uri=${redirectUri}&` +
  `approval_prompt=force&` +
  `scope=read,activity:read`;

interface User {
  show: boolean;
  color: string;
  name: string;
  total: number;
  tiles: any;
  routes: any;
  activities: number;
}

export default Vue.extend({
  name: "App",
  components: {
    UserList,
    Import,
    Export,
    Map
  },
  async mounted() {
    let response = await fetch(
      process.env.VUE_APP_API_URI + "GetActivities",
      { credentials: "include" } // cross origin cookie
    ).then(x => x.json());
    this.myRoutes = response.routes;
    this.myTiles = tilesToLatLng(response.tiles);
    this.users.push({
      name: "me",
      show: true,
      color: "#ff0000",
      total: response.tiles.length,
      activities: response.routes.length,
      routes: this.myRoutes,
      tiles: this.myTiles
    });
  },
  computed: {
    visibleUsers(): User[] {
      return this.users.filter(x => x.show);
    }
  },
  data() {
    return {
      myRoutes: [],
      myTiles: [] as number[][][],
       users: [] as User[],
      authUri: authUri
    };
  },
  methods: {
    fileImported(json: Record<string, any>) {
      let name = prompt("name?") || "";
      this.users.push({
        tiles: json.tiles,
        routes: json.routes,
        total: json.tiles.length,
        activities: json.routes.length,
        show: true,
        color: "#0000ff",
        name: name
      });
    }
  }
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
