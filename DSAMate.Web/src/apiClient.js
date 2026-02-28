const baseUrl = "https://localhost:7197/api";

function buildHeaders() {
  const headers = {
    "Content-type": "application/json",
  };

  const token = localStorage.getItem("jwt_token_storage_key");
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }
  return headers;
}

async function parseResponse(response) {
  let data = null;
  try {
    data = await response.json();
  } catch {
    data = null;
  }

  if (!response.ok) {
    const error = new Error("Request failed");
    error.response = { status: response.status, data };
    throw error;
  }

  return { data };
}

export const post = async (path, body) => {
  const response = await fetch(`${baseUrl}${path}`, {
    body: JSON.stringify(body),
    method: "POST",
    headers: buildHeaders(),
  });

  return parseResponse(response);
};

export const get = async (path) => {
  const response = await fetch(`${baseUrl}${path}`, {
    method: "GET",
    headers: buildHeaders(),
  });

  return parseResponse(response);
};
