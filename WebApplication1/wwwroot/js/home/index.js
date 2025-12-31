const { createApp } = Vue;

createApp({
  data() {
    return {
      productTitle: "台北五日遊",
      isLoading: false,
      errorMessage: "",
      brand: "",
      name: "",
      category: "",
      region: "",
      startUtc: "",
      endUtc: "",
      price: "",
      currency: "",
      description: ""
    };
  },
  computed: {
    createProductButtonStatus() {
      return this.productTitle.trim() !== "";
    },
  },
  methods: {
    async createProductViaN8n() {
      this.isLoading = true;
      // clear previous error
      this.errorMessage = "";
      try {
        const response = await fetch("/api/N8N/CreateProduct", {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            productTitle: this.productTitle,
          }),
        });
        if (response.ok) {
          const result = await response.json();
          const data = result.data;
          this.brand = data.brand ?? "";
          this.name = data.name ?? "";
          this.category = data.category ?? "";
          this.region = data.region ?? "";
          this.startUtc = data.startUtc ?? "";
          this.endUtc = data.endUtc ?? "";
          this.price = data.price ?? "";
          this.currency = data.currency ?? "";
          this.description = data.description ?? "";
        } else {
          console.error("Failed to create product");
          this.errorMessage = "建立產品失敗，請稍後再試。";
        }
      } catch (error) {
        console.error("Error:", error);
        this.errorMessage = "系統發生錯誤，請稍後再試。";
      } finally {
        this.isLoading = false;
      }
    },
  },
}).mount("#app");
