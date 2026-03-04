
async function testApi() {
  const url = 'http://localhost:8080/api/GitHub/live?page=1&pageSize=10';
  console.log(`Fetching ${url}...`);
  try {
    const response = await fetch(url);
    console.log(`Status: ${response.status}`);
    const data = await response.json();
    console.log('Response data:', JSON.stringify(data, null, 2));
  } catch (error) {
    console.error('Fetch failed:', error);
  }
}

testApi();
