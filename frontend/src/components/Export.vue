<template>
  <div>
    <button type="button" @click="download">Export</button>
    <input id="file-input" type="file" name="name" style="display: none" />
  </div>
</template>

<script>
export default {
  name: "Export",
  props: {
    content: { type: Object }
  },
  methods: {
    download() {
      let a = document.createElement("a");
      let file = new Blob([JSON.stringify(this.content)], {
        type: "text/plain"
      });
      a.href = URL.createObjectURL(file);
      a.download = "routes.json";
      a.click();
    }
  }
};
</script>

<style></style>
