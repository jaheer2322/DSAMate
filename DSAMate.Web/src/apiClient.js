const baseUrl = "https://localhost:7197/api";
const defaultHeaders = {
  "Content-type": "application/json",
};
export const post = async (path, body) => {
  const token = localStorage.getItem("jwt_token_storage_key");

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }
  const response = await fetch(`${baseUrl}${path}`, {
    body: JSON.stringify(body),
    method: "POST",
    headers: defaultHeaders,
  });

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
};
