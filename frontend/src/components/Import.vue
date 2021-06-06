<template>
  <button>
    <input
      type="file"
      @change="fileImported"
      class="input-file"
    />
    Import
  </button>
</template>

<script>
export default {
  name: "Import",
  methods: {
    fileImported(e) {
      let file = e.target.files[0];

      var reader = new FileReader();
      reader.readAsText(file);
      reader.onload = this.processFile;
    },
    processFile(e) {
      var file = e.target.result;
      let j = JSON.parse(file);
      this.$emit("file-imported", j);
    },
  },
};
</script>

<style>
.input-file {
  opacity: 0; /* invisible but it's there! */
  width: 100%;
  height: 200px;
  position: absolute;
  cursor: pointer;
}
</style>
