import { DaprClient, HttpMethod } from "@dapr/dapr";
import express from "express";
import bodyParser from "body-parser";
import path from "path";
import { fileURLToPath } from "url";

const app = express();
app.use(bodyParser.json());

const port = Number(process.env.PORT) || 8080;
const client = new DaprClient();

await client.start();

app.get("/healthz", (req, res) => {
  res.sendStatus(200);
});

app.listen(port, () => {
  console.log(`Node subscriber server running on port: ${port}`);
});
app.post("/api/review", async (req, res) => {
  const serviceApp = "review";
  const serviceAppMethod = "Create";

  const response = await client.invoker.invoke(
    serviceApp,
    serviceAppMethod,
    HttpMethod.POST,
    req.body
  );

  return res.send(response);
});

app.get("/api/geolocator", async (req, res) => {
  try {
    const serviceApp = "locator";
    const serviceAppMethod = `around/${req.query.city}/${req.query.lat}/${req.query.lng}`;

    const response = await client.invoker.invoke(
      serviceApp,
      serviceAppMethod,
      HttpMethod.GET
    );

    res.send(response);
  } catch (err) {
    console.log(err);
    res.send(500, err);
  }
});
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
// Serve static files
app.use(express.static(path.join(__dirname, "client/build")));

app.get("/", function (req, res) {
  console.log(req);
  res.sendFile(path.join(__dirname, "client/build", "index.html"));
});

app.get("/review", function (req, res) {
  console.log(req);
  res.sendFile(path.join(__dirname, "client/build", "index.html"));
});

console.log("Node  App Started");
export default app;
