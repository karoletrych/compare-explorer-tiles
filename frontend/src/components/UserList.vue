<template>
  <table class="items">
    <tr>
      <th>name</th>
      <th>show?</th>
      <th>activities</th>
      <th>tiles</th>
      <th>color</th>
    </tr>
    <tr v-for="i in value" :key="i.name">
      <td>
        {{ i.name }}
      </td>
      <td>
        <input v-model="i.show" type="checkbox" />
      </td>
      <td>
        {{ i.activities }}
      </td>
      <td>
        {{ i.total }}
      </td>
      <td>
        <input type="color" v-model="i.color" />
      </td>
    </tr>
  </table>
</template>

<script lang="ts">
import Vue from "vue";

export default Vue.extend({
  name: "UserList",
  props: {
    value: { type: Array }
  },
  data() {
    return {};
  }
});
</script>

<style>
.items td,
.items th {
  border: 1px solid #ddd;
  padding: 8px;
}

.items tr:hover {
  background-color: #ddd;
}
</style>
