interface Tile {
  x: number;
  y: number;
}

export interface User {
    show: boolean;
    color: string;
    name: string;
    total: number;
    tiles: any;
    routes: any;
    activities: number;
  }


const DIV = Math.pow(2, 14);

export function tile2long(t: number) {
  return (t / DIV) * 360 - 180;
}
export function tile2lat(t: number) {
  let n = Math.PI - (2 * Math.PI * t) / DIV;
  return (180 / Math.PI) * Math.atan(0.5 * (Math.exp(n) - Math.exp(-n)));
}
export function deg2rad(t: number) {
  return t * (Math.PI / 180);
}

export function calcTile(lat: number, lng: number) {
  return {
    x: Math.floor(((lng + 180) / 360) * DIV),
    y: Math.floor(
      ((1 -
        Math.log(Math.tan(deg2rad(lat)) + 1 / Math.cos(deg2rad(lat))) /
          Math.PI) /
        2) *
        DIV
    )
  };
}

export function tilesToLatLng(tiles: Tile[]) {
  let tileCoords = tiles.map(tile => {
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
      [i, j]
    ];
  });
  return tileCoords;
}
