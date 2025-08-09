// Helpers / State
let map, drawn, currentLayer = null;
let drawControl;
const $ = (id) => document.getElementById(id);
const apiStorageKey = 'farm_polygon_api_base';

function toast(msg, isError=false) {
  const el = $('toast');
  el.textContent = msg;
  el.style.background = isError ? 'rgba(185, 28, 28, .95)' : 'rgba(17, 24, 39, .95)';
  el.style.display = 'block';
  setTimeout(() => el.style.display = 'none', 2200);
}

function km2(v){ return (v/1_000_000).toFixed(3); }
function m2(v){ return v.toFixed(0); }
function km(v){ return (v/1000).toFixed(3); }
function m(v){ return v.toFixed(0); }

function getApiBase() {
  return ($('apiBase').value?.trim()) || window.localStorage.getItem(apiStorageKey) || window.location.origin;
}
function saveApiBase() {
  const v = $('apiBase').value?.trim();
  if (v) { window.localStorage.setItem(apiStorageKey, v); toast('API base salva.'); }
  else { toast('Informe uma URL.', true); }
}
function resetApiBase() {
  $('apiBase').value = ''; window.localStorage.removeItem(apiStorageKey); toast('API base limpa.');
}

// Init
function init() {
  $('apiBase').value = window.localStorage.getItem(apiStorageKey) || '';

  map = L.map('map', { zoomControl: true }).setView([-15.7801, -47.9292], 5);
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; OpenStreetMap contributors'
  }).addTo(map);

  drawn = new L.FeatureGroup();
  map.addLayer(drawn);

  drawControl = new L.Control.Draw({
    draw: { polygon: { allowIntersection: false, showArea: false },
            marker: false, circle: false, circlemarker: false, rectangle: false, polyline: false },
    edit: { featureGroup: drawn, edit: true, remove: true }
  });
  map.addControl(drawControl);

  map.on(L.Draw.Event.CREATED, (e) => {
    if (currentLayer) drawn.removeLayer(currentLayer);
    currentLayer = e.layer;
    drawn.addLayer(currentLayer);
    updateMetrics();
  });
  map.on(L.Draw.Event.EDITED, (e) => {
    e.layers.eachLayer(l => { currentLayer = l; });
    updateMetrics();
  });

  $('drawBtn').addEventListener('click', () => new L.Draw.Polygon(map, drawControl.options.draw.polygon).enable());
  $('editBtn').addEventListener('click', () => new L.EditToolbar.Edit(map, { featureGroup: drawn }).enable());
  $('clearBtn').addEventListener('click', clearPolygon);
  $('saveBtn').addEventListener('click', savePolygon);
  $('loadBtn').addEventListener('click', loadPolygonById);
  $('exportBtn').addEventListener('click', exportGeoJSON);
  $('importInput').addEventListener('change', importGeoJSONFile);
  $('listBtn').addEventListener('click', listFarms);
  $('saveApiBtn').addEventListener('click', saveApiBase);
  $('resetApiBtn').addEventListener('click', resetApiBase);
}

function clearPolygon() {
  if (currentLayer) { drawn.removeLayer(currentLayer); currentLayer = null; }
  $('metrics').textContent = 'Área: — | Perímetro: —';
}

function ringFromLayer(layer) {
  const latlngs = layer.getLatLngs();
  const ring = Array.isArray(latlngs) ? (Array.isArray(latlngs[0]) ? latlngs[0] : latlngs) : latlngs;
  return ring;
}

function updateMetrics() {
  if (!currentLayer) return;
  const ring = ringFromLayer(currentLayer);
  if (!ring || ring.length < 3) return;

  // GeoJSON polygon (lng,lat)
  const coords = ring.map(p => [p.lng, p.lat]);
  coords.push(coords[0]); // fechar
  const poly = turf.polygon([coords]);

  const areaM2 = turf.area(poly);
  const perimeterM = turf.length(poly, {units: 'kilometers'}) * 1000; // turf.length em km
  $('metrics').textContent = `Área: ${km2(areaM2)} km² (${m2(areaM2)} m²) | Perímetro: ${km(perimeterM)} km (${m(perimeterM)} m)`;
}

function savePolygon() {
  const name = $('farmName').value || 'Minha Fazenda';
  if (!currentLayer) return toast('Desenhe um polígono antes de salvar.', true);

  const ring = ringFromLayer(currentLayer);
  if (!ring || ring.length < 3) return toast('Polígono muito pequeno.', true);

  const coords = ring.map(p => ({ lat: p.lat, lng: p.lng }));
  if (coords.length > 0) coords.push(coords[0]);

  const api = getApiBase();
  fetch(`${api}/api/farms`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name, coordinates: coords })
  })
  .then(async res => {
    if (!res.ok) throw new Error(await res.text());
    return res.json();
  })
  .then(data => toast('Salvo! ID: ' + data.id))
  .catch(err => toast('Erro: ' + err.message, true));
}

function loadPolygonById() {
  const id = $('farmId').value;
  if (!id) return toast('Informe um ID.', true);

  const api = getApiBase();
  fetch(`${api}/api/farms/${id}`)
    .then(async res => {
      if (!res.ok) throw new Error(await res.text());
      return res.json();
    })
    .then(data => {
      if (currentLayer) drawn.removeLayer(currentLayer);
      const path = data.coordinates.map(c => [c.lat, c.lng]);
      currentLayer = L.polygon(path);
      drawn.addLayer(currentLayer);

      map.fitBounds(currentLayer.getBounds());
      $('farmName').value = data.name || '';
      updateMetrics();
      toast('Carregado: ' + data.name);
    })
    .catch(err => toast('Erro: ' + err.message, true));
}

function exportGeoJSON() {
  if (!currentLayer) return toast('Nada para exportar.', true);
  const ring = ringFromLayer(currentLayer);
  const coords = ring.map(p => [p.lng, p.lat]);
  coords.push(coords[0]);

  const geojson = {
    type: "Feature",
    geometry: { type: "Polygon", coordinates: [coords] },
    properties: { name: $('farmName').value || 'Minha Fazenda' }
  };
  $('geojsonText').value = JSON.stringify(geojson, null, 2);
  toast('GeoJSON exportado.');
}

function importGeoJSONFile(e) {
  const file = e.target.files?.[0];
  if (!file) return;
  const reader = new FileReader();
  reader.onload = () => {
    try {
      const data = JSON.parse(reader.result);
      importGeoJSON(data);
      toast('GeoJSON importado.');
    } catch (err) {
      toast('Arquivo inválido.', true);
    }
  };
  reader.readAsText(file);
}

function importGeoJSON(obj) {
  try {
    const geom = obj.type === 'Feature' ? obj.geometry : obj;
    if (!geom || geom.type !== 'Polygon') throw new Error('Somente Polygon.');
    const coords = geom.coordinates?.[0];
    if (!coords || coords.length < 4) throw new Error('Polígono inválido.');

    const path = coords.map(([lng, lat]) => [lat, lng]);
    if (currentLayer) drawn.removeLayer(currentLayer);
    currentLayer = L.polygon(path);
    drawn.addLayer(currentLayer);
    map.fitBounds(currentLayer.getBounds());

    $('farmName').value = obj.properties?.name || '';
    updateMetrics();
  } catch (e) {
    toast('GeoJSON inválido: ' + e.message, true);
  }
}

// ---- FIX: Lista rápida ----
function listFarms() {
  const ul = $('farmsList');
  ul.innerHTML = '<li>Carregando...</li>';
  const api = getApiBase();
  fetch(`${api}/api/farms`)
    .then(async res => {
      if (!res.ok) throw new Error(await res.text());
      return res.json();
    })
    .then(items => {
      ul.innerHTML = '';
      if (!Array.isArray(items) || items.length === 0) {
        ul.innerHTML = '<li>Nenhuma fazenda encontrada.</li>';
        return;
      }
      items.forEach(it => {
        const li = document.createElement('li');
        li.textContent = `#${it.id} — ${it.name}`;
        li.title = 'Clique para carregar';
        li.addEventListener('click', () => {
          $('farmId').value = it.id;
          loadPolygonById();
        });
        ul.appendChild(li);
      });
    })
    .catch(err => {
      ul.innerHTML = `<li>Erro ao listar: ${err.message}</li>`;
    });
}

document.addEventListener('DOMContentLoaded', init);
