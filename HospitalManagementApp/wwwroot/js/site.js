const HM_MOTION = {
	reduced: window.matchMedia("(prefers-reduced-motion: reduce)").matches
};

document.addEventListener("DOMContentLoaded", () => {
	initPurposefulAnimations();
	initCustomDateTimePickers();
	initAjaxListSearch();
	initAjaxDropdowns();
	initGlobalSearch();
	initClientValidation();
});

function initCustomDateTimePickers() {
	const browserLocale = (navigator.languages && navigator.languages.length > 0 ? navigator.languages[0] : navigator.language || "en").toLowerCase();
	const isCroatian = browserLocale.startsWith("hr");
	const locale = isCroatian ? "hr-HR" : "en-US";
	const weekStartsMonday = isCroatian;
	const uiText = isCroatian
		? {
			today: "Danas",
			now: "Sada",
			clear: "Ocisti",
			empty: "Odabir nije postavljen",
			hintDate: "Format datuma: dd.mm.gggg.",
			hintDateTime: "Format datuma i vremena: dd.mm.gggg. HH:mm",
			time: "Vrijeme",
			pickDate: "Odaberi datum",
			pickDateTime: "Odaberi datum i vrijeme"
		}
		: {
			today: "Today",
			now: "Now",
			clear: "Clear",
			empty: "No date selected",
			hintDate: "Date format: mm/dd/yyyy",
			hintDateTime: "Date and time format: mm/dd/yyyy hh:mm",
			time: "Time",
			pickDate: "Pick a date",
			pickDateTime: "Pick date and time"
		};

	const pickers = document.querySelectorAll("[data-dtp='true']");
	let openPicker = null;

	const padNumber = (value) => String(value).padStart(2, "0");
	const toIsoDate = (date) => `${date.getFullYear()}-${padNumber(date.getMonth() + 1)}-${padNumber(date.getDate())}`;
	const normalizeDate = (date) => new Date(date.getFullYear(), date.getMonth(), date.getDate());
	const parseHiddenValue = (value, dateOnly) => {
		if (!value) {
			return null;
		}

		const [datePart, timePart] = value.split("T");
		const [year, month, day] = (datePart || "").split("-").map(Number);
		if (!year || !month || !day) {
			return null;
		}

		const result = {
			selectedDate: new Date(year, month - 1, day),
			hour: 0,
			minute: 0
		};

		if (!dateOnly && timePart) {
			const [hour, minute] = timePart.split(":").map(Number);
			result.hour = Number.isFinite(hour) ? hour : 0;
			result.minute = Number.isFinite(minute) ? minute : 0;
		}

		return result;
	};

	const buildWeekdayLabels = () => {
		const baseMonday = new Date(2026, 0, 5);
		const formatter = new Intl.DateTimeFormat(locale, { weekday: "short" });
		return Array.from({ length: 7 }, (_, index) => {
			const offset = weekStartsMonday ? index : (index + 6) % 7;
			const day = new Date(baseMonday);
			day.setDate(baseMonday.getDate() + offset);
			return formatter.format(day);
		});
	};

	const weekdayLabels = buildWeekdayLabels();

	const closePicker = (picker) => {
		if (!picker) {
			return;
		}

		picker.classList.remove("is-open");
		const trigger = picker.querySelector(".hm-dtp__trigger");
		if (trigger) {
			trigger.setAttribute("aria-expanded", "false");
		}
		if (openPicker === picker) {
			openPicker = null;
		}
	};

	const openCurrentPicker = (picker) => {
		if (openPicker && openPicker !== picker) {
			closePicker(openPicker);
		}
		picker.classList.add("is-open");
		const trigger = picker.querySelector(".hm-dtp__trigger");
		if (trigger) {
			trigger.setAttribute("aria-expanded", "true");
		}
		openPicker = picker;
	};

	pickers.forEach((picker) => {
		const hiddenInput = picker.querySelector(".hm-dtp__value");
		const trigger = picker.querySelector(".hm-dtp__trigger");
		const triggerValue = picker.querySelector("[data-dtp-trigger-value='true']");
		const panel = picker.querySelector("[data-dtp-panel='true']");
		const monthLabel = picker.querySelector("[data-dtp-month='true']");
		const calendar = picker.querySelector("[data-dtp-calendar='true']");
		const weekdays = picker.querySelector("[data-dtp-weekdays='true']");
		const hourSelect = picker.querySelector("[data-dtp-hour='true']");
		const minuteSelect = picker.querySelector("[data-dtp-minute='true']");
		const timeLabel = picker.querySelector(".hm-dtp__time-label");
		const hint = picker.querySelector("[data-dtp-hint='true']");
		const preview = picker.querySelector("[data-dtp-preview='true']");
		const dateOnly = picker.getAttribute("data-date-only") === "true";

		if (!hiddenInput || !trigger || !panel || !monthLabel || !calendar || !weekdays || !triggerValue) {
			return;
		}

		const parsedInitial = parseHiddenValue(hiddenInput.value, dateOnly);
		const state = {
			selectedDate: parsedInitial?.selectedDate ? normalizeDate(parsedInitial.selectedDate) : null,
			hour: parsedInitial?.hour ?? 0,
			minute: parsedInitial?.minute ?? 0,
			viewYear: parsedInitial?.selectedDate?.getFullYear() ?? new Date().getFullYear(),
			viewMonth: parsedInitial?.selectedDate?.getMonth() ?? new Date().getMonth()
		};

		if (hint) {
			hint.textContent = dateOnly ? uiText.hintDate : uiText.hintDateTime;
		}
		if (timeLabel) {
			timeLabel.textContent = uiText.time;
		}

		picker.querySelectorAll("[data-dtp-action='today']").forEach((button) => {
			button.textContent = uiText.today;
		});
		picker.querySelectorAll("[data-dtp-action='now']").forEach((button) => {
			button.textContent = uiText.now;
		});
		picker.querySelectorAll("[data-dtp-action='clear']").forEach((button) => {
			button.textContent = uiText.clear;
		});

		const emitFieldEvents = () => {
			hiddenInput.dispatchEvent(new Event("input", { bubbles: true }));
			hiddenInput.dispatchEvent(new Event("change", { bubbles: true }));
		};

		const formatSelectedValue = () => {
			if (!state.selectedDate) {
				return dateOnly ? uiText.pickDate : uiText.pickDateTime;
			}

			const dateInstance = new Date(
				state.selectedDate.getFullYear(),
				state.selectedDate.getMonth(),
				state.selectedDate.getDate(),
				dateOnly ? 0 : state.hour,
				dateOnly ? 0 : state.minute
			);

			return new Intl.DateTimeFormat(locale, dateOnly
				? { year: "numeric", month: "2-digit", day: "2-digit" }
				: { year: "numeric", month: "2-digit", day: "2-digit", hour: "2-digit", minute: "2-digit" })
				.format(dateInstance);
		};

		const updatePreview = () => {
			if (!preview) {
				return;
			}

			if (!state.selectedDate) {
				preview.textContent = uiText.empty;
				return;
			}

			preview.textContent = formatSelectedValue();
		};

		const updateTrigger = () => {
			triggerValue.textContent = formatSelectedValue();
			trigger.classList.toggle("is-empty", !state.selectedDate);
		};

		const syncTimeSelectors = () => {
			if (!hourSelect || !minuteSelect) {
				return;
			}

			hourSelect.value = padNumber(state.hour);
			minuteSelect.value = padNumber(state.minute);
		};

		const syncHiddenInput = () => {
			if (!state.selectedDate) {
				hiddenInput.value = "";
				updateTrigger();
				updatePreview();
				emitFieldEvents();
				return;
			}

			const dateValue = toIsoDate(state.selectedDate);

			if (dateOnly) {
				hiddenInput.value = dateValue;
				updateTrigger();
				updatePreview();
				emitFieldEvents();
				return;
			}

			const timeValue = `${padNumber(state.hour)}:${padNumber(state.minute)}`;
			hiddenInput.value = timeValue ? `${dateValue}T${timeValue}` : "";
			updateTrigger();
			updatePreview();
			emitFieldEvents();
		};

		const renderWeekdays = () => {
			weekdays.innerHTML = weekdayLabels
				.map((label) => `<span class="hm-dtp__weekday">${label}</span>`)
				.join("");
		};

		const renderCalendar = () => {
			const firstDay = new Date(state.viewYear, state.viewMonth, 1);
			const lastDay = new Date(state.viewYear, state.viewMonth + 1, 0);
			const today = normalizeDate(new Date());
			const firstWeekday = firstDay.getDay();
			const leading = weekStartsMonday ? (firstWeekday + 6) % 7 : firstWeekday;
			const totalSlots = Math.ceil((leading + lastDay.getDate()) / 7) * 7;

			monthLabel.textContent = new Intl.DateTimeFormat(locale, { month: "long", year: "numeric" }).format(firstDay);
			calendar.innerHTML = "";

			for (let index = 0; index < totalSlots; index += 1) {
				const dayNumber = index - leading + 1;
				if (dayNumber < 1 || dayNumber > lastDay.getDate()) {
					const filler = document.createElement("span");
					filler.className = "hm-dtp__day hm-dtp__day--filler";
					calendar.appendChild(filler);
					continue;
				}

				const date = new Date(state.viewYear, state.viewMonth, dayNumber);
				const button = document.createElement("button");
				button.type = "button";
				button.className = "hm-dtp__day";
				button.textContent = String(dayNumber);
				button.dataset.dateValue = toIsoDate(date);

				if (state.selectedDate && toIsoDate(state.selectedDate) === toIsoDate(date)) {
					button.classList.add("is-selected");
				}
				if (toIsoDate(today) === toIsoDate(date)) {
					button.classList.add("is-today");
				}

				button.addEventListener("click", () => {
					state.selectedDate = normalizeDate(date);
					renderCalendar();
					syncHiddenInput();
					if (dateOnly) {
						closePicker(picker);
					}
				});

				calendar.appendChild(button);
			}
		};

		const setToday = () => {
			const now = new Date();
			state.selectedDate = normalizeDate(now);
			state.viewYear = now.getFullYear();
			state.viewMonth = now.getMonth();
			renderCalendar();
			syncHiddenInput();
		};

		const setNow = () => {
			const now = new Date();
			state.selectedDate = normalizeDate(now);
			state.viewYear = now.getFullYear();
			state.viewMonth = now.getMonth();
			state.hour = now.getHours();
			state.minute = now.getMinutes();
			syncTimeSelectors();
			renderCalendar();
			syncHiddenInput();
		};

		const clearValues = () => {
			state.selectedDate = null;
			state.hour = 0;
			state.minute = 0;
			syncTimeSelectors();
			renderCalendar();
			hiddenInput.value = "";
			updateTrigger();
			updatePreview();
			emitFieldEvents();
		};

		if (hourSelect && minuteSelect) {
			hourSelect.innerHTML = Array.from({ length: 24 }, (_, hour) => `<option value="${padNumber(hour)}">${padNumber(hour)}</option>`).join("");
			minuteSelect.innerHTML = Array.from({ length: 60 }, (_, minute) => `<option value="${padNumber(minute)}">${padNumber(minute)}</option>`).join("");
			hourSelect.addEventListener("change", () => {
				state.hour = Number(hourSelect.value);
				syncHiddenInput();
			});
			minuteSelect.addEventListener("change", () => {
				state.minute = Number(minuteSelect.value);
				syncHiddenInput();
			});
		}

		trigger.addEventListener("click", () => {
			if (picker.classList.contains("is-open")) {
				closePicker(picker);
			} else {
				openCurrentPicker(picker);
			}
		});

		picker.querySelectorAll("[data-dtp-nav]").forEach((button) => {
			button.addEventListener("click", () => {
				const direction = button.getAttribute("data-dtp-nav");
				const delta = direction === "prev" ? -1 : 1;
				const nextMonth = new Date(state.viewYear, state.viewMonth + delta, 1);
				state.viewYear = nextMonth.getFullYear();
				state.viewMonth = nextMonth.getMonth();
				renderCalendar();
			});
		});

		picker.querySelectorAll("[data-dtp-action]").forEach((button) => {
			button.addEventListener("click", () => {
				const action = button.getAttribute("data-dtp-action");
				if (action === "today") {
					setToday();
				} else if (action === "now") {
					setNow();
				} else if (action === "clear") {
					clearValues();
				}
			});
		});

		renderWeekdays();
		syncTimeSelectors();
		renderCalendar();
		syncHiddenInput();
	});

	document.addEventListener("click", (event) => {
		if (openPicker && !openPicker.contains(event.target)) {
			closePicker(openPicker);
		}
	});
}

function animateWithWebApi(element, keyframes, options) {
	if (!element || HM_MOTION.reduced || typeof element.animate !== "function") {
		return null;
	}

	return element.animate(keyframes, options);
}

function initPurposefulAnimations() {
	initAmbientParallax();
	initInteractiveCards();
	initButtonEffects();
	initViewportRevealAnimations();
	initKpiCountUpAnimations();
}

function initAmbientParallax() {
	if (HM_MOTION.reduced) {
		return;
	}

	const ambient = document.querySelector(".hm-ambient");
	if (!ambient) {
		return;
	}

	let rafId = 0;

	window.addEventListener("pointermove", (event) => {
		if (rafId) {
			cancelAnimationFrame(rafId);
		}

		rafId = requestAnimationFrame(() => {
			const x = (event.clientX / window.innerWidth - 0.5) * 24;
			const y = (event.clientY / window.innerHeight - 0.5) * 24;
			document.documentElement.style.setProperty("--hm-mx", `${x}px`);
			document.documentElement.style.setProperty("--hm-my", `${y}px`);
		});
	});
}

function initInteractiveCards() {
	if (HM_MOTION.reduced || window.matchMedia("(pointer: coarse)").matches) {
		return;
	}

	const cards = document.querySelectorAll(".hm-card, .hm-detail-card, .hm-record-card, .hm-action-card, .hm-kpi-card");
	cards.forEach((card) => {
		card.classList.add("hm-tilt-card");

		card.addEventListener("pointermove", (event) => {
			const rect = card.getBoundingClientRect();
			const px = (event.clientX - rect.left) / rect.width;
			const py = (event.clientY - rect.top) / rect.height;
			const rotateY = (px - 0.5) * 6;
			const rotateX = (0.5 - py) * 6;
			card.style.setProperty("--hm-tilt-x", `${rotateX.toFixed(2)}deg`);
			card.style.setProperty("--hm-tilt-y", `${rotateY.toFixed(2)}deg`);
		});

		card.addEventListener("pointerleave", () => {
			card.style.setProperty("--hm-tilt-x", "0deg");
			card.style.setProperty("--hm-tilt-y", "0deg");
		});
	});
}

function initButtonEffects() {
	const buttons = document.querySelectorAll(".btn, .hm-btn-primary, .hm-btn-secondary");
	buttons.forEach((button) => {
		button.addEventListener("pointerdown", (event) => {
			const rect = button.getBoundingClientRect();
			const ripple = document.createElement("span");
			ripple.className = "hm-btn-ripple";
			ripple.style.left = `${event.clientX - rect.left}px`;
			ripple.style.top = `${event.clientY - rect.top}px`;
			button.appendChild(ripple);
			window.setTimeout(() => ripple.remove(), 650);
		});

		if (HM_MOTION.reduced || window.matchMedia("(pointer: coarse)").matches) {
			return;
		}

		button.addEventListener("pointermove", (event) => {
			const rect = button.getBoundingClientRect();
			const x = ((event.clientX - rect.left) / rect.width - 0.5) * 8;
			const y = ((event.clientY - rect.top) / rect.height - 0.5) * 8;
			button.style.transform = `translate(${x}px, ${y - 2}px)`;
		});

		button.addEventListener("pointerleave", () => {
			button.style.transform = "";
		});
	});
}

function initViewportRevealAnimations() {
	if (HM_MOTION.reduced || typeof IntersectionObserver === "undefined") {
		return;
	}

	const revealTargets = document.querySelectorAll(
		".hm-page-header, .hm-section-heading, .hm-card, .hm-detail-card, .hm-table-wrap, .hm-action-card, .hm-ops-card, .hm-kpi-card, .hm-record-card"
	);

	revealTargets.forEach((target) => {
		target.classList.add("hm-motion-ready");
	});

	const observer = new IntersectionObserver((entries, obs) => {
		entries.forEach((entry) => {
			if (!entry.isIntersecting) {
				return;
			}

			entry.target.classList.add("hm-motion-in");
			animateWithWebApi(
				entry.target,
				[
					{ opacity: 0, transform: "translateY(12px) scale(0.985)" },
					{ opacity: 1, transform: "translateY(0) scale(1)" }
				],
				{ duration: 420, easing: "cubic-bezier(.2,.9,.2,1)", fill: "both" }
			);

			obs.unobserve(entry.target);
		});
	}, { threshold: 0.15 });

	revealTargets.forEach((target) => observer.observe(target));
}

function initKpiCountUpAnimations() {
	if (HM_MOTION.reduced || typeof IntersectionObserver === "undefined") {
		return;
	}

	const values = document.querySelectorAll(".hm-kpi-value");

	const observer = new IntersectionObserver((entries, obs) => {
		entries.forEach((entry) => {
			if (!entry.isIntersecting) {
				return;
			}

			const rawText = entry.target.textContent?.trim() || "";
			const normalized = rawText.replace(/,/g, "");
			if (!/^-?\d+(\.\d+)?$/.test(normalized)) {
				obs.unobserve(entry.target);
				return;
			}

			const targetValue = Number(normalized);
			const start = performance.now();
			const duration = 850;

			const tick = (now) => {
				const progress = Math.min((now - start) / duration, 1);
				const eased = 1 - Math.pow(1 - progress, 3);
				const value = Math.round(targetValue * eased);
				entry.target.textContent = new Intl.NumberFormat().format(value);

				if (progress < 1) {
					requestAnimationFrame(tick);
				}
			};

			requestAnimationFrame(tick);
			obs.unobserve(entry.target);
		});
	}, { threshold: 0.5 });

	values.forEach((value) => observer.observe(value));
}

async function swapSearchResultsWithMotion(target, html) {
	if (!target) {
		return;
	}

	if (!HM_MOTION.reduced && typeof document.startViewTransition === "function") {
		const transition = document.startViewTransition(() => {
			target.innerHTML = html;
		});

		try {
			await transition.finished;
		} catch {
			// Fall back silently if transition cannot complete.
		}
	} else {
		target.innerHTML = html;
		animateWithWebApi(
			target,
			[
				{ opacity: 0.55, transform: "translateY(4px)" },
				{ opacity: 1, transform: "translateY(0)" }
			],
			{ duration: 220, easing: "ease-out" }
		);
	}

	const resultItems = target.querySelectorAll("tbody tr, .hm-card, .hm-record-card");
	resultItems.forEach((item, index) => {
		animateWithWebApi(
			item,
			[
				{ opacity: 0, transform: "translateY(10px)" },
				{ opacity: 1, transform: "translateY(0)" }
			],
			{ duration: 260, delay: Math.min(index * 28, 240), easing: "cubic-bezier(.2,.8,.2,1)", fill: "both" }
		);
	});
}

function setSearchTargetBusy(target, isBusy) {
	if (!target) {
		return;
	}

	target.classList.add("hm-search-target");
	target.classList.toggle("is-loading", isBusy);
	target.setAttribute("aria-busy", isBusy ? "true" : "false");
}

function initClientValidation() {
	if (!window.jQuery || !window.jQuery.validator) {
		return;
	}

	const $ = window.jQuery;

	$("form").each(function () {
		const $form = $(this);

		$.validator.unobtrusive.parse($form);
		const validator = $form.data("validator");

		if (!validator) {
			return;
		}

		$form.find(".hm-ajax-dropdown input[type='hidden'][data-val='true']").addClass("hm-validate-hidden");
		$form.find(".hm-dtp input[type='hidden'][name][id]").addClass("hm-validate-hidden");

		if (typeof validator.settings.ignore === "string" && validator.settings.ignore.includes(":hidden")) {
			validator.settings.ignore = validator.settings.ignore.replace(":hidden", ":hidden:not(.hm-validate-hidden)");
		}

		validator.settings.onfocusout = function (element) {
			this.element(element);
		};

		const defaultHighlight = validator.settings.highlight;
		validator.settings.highlight = function (element, errorClass, validClass) {
			if (typeof defaultHighlight === "function") {
				defaultHighlight.call(this, element, errorClass, validClass);
			} else {
				$(element).addClass(errorClass);
			}

			const $element = $(element);
			if ($element.hasClass("hm-validate-hidden")) {
				const $dropdown = $element.closest(".hm-ajax-dropdown");
				if ($dropdown.length > 0) {
					$dropdown
						.addClass("is-invalid")
						.find(".hm-ajax-dropdown__input")
						.addClass(errorClass);
				}

				const $dateTimePicker = $element.closest(".hm-dtp");
				if ($dateTimePicker.length > 0) {
					$dateTimePicker
						.addClass("is-invalid")
						.find(".hm-dtp__trigger")
						.addClass(errorClass);
				}
			}
		};

		const defaultUnhighlight = validator.settings.unhighlight;
		validator.settings.unhighlight = function (element, errorClass, validClass) {
			if (typeof defaultUnhighlight === "function") {
				defaultUnhighlight.call(this, element, errorClass, validClass);
			} else {
				$(element).removeClass(errorClass);
			}

			const $element = $(element);
			if ($element.hasClass("hm-validate-hidden")) {
				const $dropdown = $element.closest(".hm-ajax-dropdown");
				if ($dropdown.length > 0) {
					$dropdown
						.removeClass("is-invalid")
						.find(".hm-ajax-dropdown__input")
						.removeClass(errorClass);
				}

				const $dateTimePicker = $element.closest(".hm-dtp");
				if ($dateTimePicker.length > 0) {
					$dateTimePicker
						.removeClass("is-invalid")
						.find(".hm-dtp__trigger")
						.removeClass(errorClass);
				}
			}
		};

		$form.on("blur", ".hm-ajax-dropdown__input", function () {
			const hiddenInput = this.closest(".hm-ajax-dropdown")?.querySelector("input[type='hidden'][data-val='true']");
			if (hiddenInput) {
				validator.element(hiddenInput);
			}
		});

		$form.on("change", ".hm-ajax-dropdown input[type='hidden'][data-val='true']", function () {
			validator.element(this);
		});

		$form.on("submit", function () {
			requestAnimationFrame(() => {
				const firstInvalid = this.querySelector(".input-validation-error, .hm-ajax-dropdown.is-invalid .hm-ajax-dropdown__input");
				if (!firstInvalid) {
					return;
				}

				firstInvalid.scrollIntoView({
					behavior: HM_MOTION.reduced ? "auto" : "smooth",
					block: "center"
				});

				if (typeof firstInvalid.focus === "function") {
					firstInvalid.focus({ preventScroll: true });
				}

				firstInvalid.classList.remove("hm-attention-pulse");
				void firstInvalid.offsetWidth;
				firstInvalid.classList.add("hm-attention-pulse");
			});
		});
	});
}

function initAjaxListSearch() {
	const searchInputs = document.querySelectorAll("[data-ajax-search='true']");

	searchInputs.forEach((input) => {
		const searchUrl = input.getAttribute("data-search-url");
		const targetSelector = input.getAttribute("data-search-target");
		const target = targetSelector ? document.querySelector(targetSelector) : null;

		if (!searchUrl || !target) {
			return;
		}

		let debounceTimer;
		let activeController;

		input.addEventListener("input", () => {
			clearTimeout(debounceTimer);
			debounceTimer = setTimeout(async () => {
				const term = input.value || "";
				const url = `${searchUrl}?term=${encodeURIComponent(term)}`;

				if (activeController) {
					activeController.abort();
				}

				activeController = new AbortController();
				setSearchTargetBusy(target, true);

				try {
					const response = await fetch(url, {
						method: "GET",
						signal: activeController.signal,
						headers: {
							"X-Requested-With": "XMLHttpRequest"
						}
					});

					if (!response.ok) {
						return;
					}

					const html = await response.text();
					await swapSearchResultsWithMotion(target, html);
				} catch {
					if (activeController.signal.aborted) {
						return;
					}

					// Keep UI stable if search endpoint temporarily fails.
				} finally {
					setSearchTargetBusy(target, false);
				}
			}, 250);
		});
	});
}

function initAjaxDropdowns() {
	const dropdowns = document.querySelectorAll("[data-ajax-dropdown='true']");

	dropdowns.forEach((dropdown) => {
		const searchUrl = dropdown.getAttribute("data-search-url");
		const hiddenInput = dropdown.querySelector("input[type='hidden']");
		const textInput = dropdown.querySelector(".hm-ajax-dropdown__input");
		const menu = dropdown.querySelector(".hm-ajax-dropdown__menu");

		if (!searchUrl || !hiddenInput || !textInput || !menu) {
			return;
		}

		let items = [];
		let highlightedIndex = -1;
		let debounceTimer;
		let activeController;

		const closeMenu = () => {
			menu.classList.remove("show");
			highlightedIndex = -1;
		};

		const openMenu = () => {
			menu.classList.add("show");
		};

		const updateHighlight = () => {
			const buttons = menu.querySelectorAll(".hm-ajax-dropdown__option");
			buttons.forEach((button, index) => {
				button.classList.toggle("active", index === highlightedIndex);
			});
		};

		const selectItem = (index) => {
			if (index < 0 || index >= items.length) {
				return;
			}

			const item = items[index];
			hiddenInput.value = item.value;
			hiddenInput.dispatchEvent(new Event("change", { bubbles: true }));
			textInput.value = item.label;
			closeMenu();
		};

		const renderItems = () => {
			if (items.length === 0) {
				menu.innerHTML = '<div class="hm-ajax-dropdown__empty">No results found</div>';
				openMenu();
				return;
			}

			menu.innerHTML = items
				.map((item) => `<button type="button" class="hm-ajax-dropdown__option" data-value="${item.value}">${item.label}</button>`)
				.join("");

			const buttons = menu.querySelectorAll(".hm-ajax-dropdown__option");
			buttons.forEach((button, index) => {
				button.addEventListener("mousedown", (event) => {
					event.preventDefault();
				});

				button.addEventListener("click", () => {
					selectItem(index);
				});

				animateWithWebApi(
					button,
					[
						{ opacity: 0, transform: "translateY(8px)" },
						{ opacity: 1, transform: "translateY(0)" }
					],
					{ duration: 180, delay: Math.min(index * 20, 180), easing: "ease-out", fill: "both" }
				);
			});

			highlightedIndex = 0;
			updateHighlight();
			openMenu();
		};

		const fetchItems = async (term) => {
			const url = `${searchUrl}?term=${encodeURIComponent(term || "")}`;

			if (activeController) {
				activeController.abort();
			}

			activeController = new AbortController();

			try {
				const response = await fetch(url, {
					method: "GET",
					signal: activeController.signal,
					headers: {
						"X-Requested-With": "XMLHttpRequest"
					}
				});

				if (!response.ok) {
					return;
				}

				const data = await response.json();
				if (!Array.isArray(data)) {
					return;
				}

				items = data;
				renderItems();
			} catch {
				if (activeController.signal.aborted) {
					return;
				}

				menu.innerHTML = '<div class="hm-ajax-dropdown__empty">Unable to load data</div>';
				openMenu();
			}
		};

		textInput.addEventListener("focus", () => {
			fetchItems(textInput.value);
		});

		textInput.addEventListener("input", () => {
			hiddenInput.value = "";
			clearTimeout(debounceTimer);
			debounceTimer = setTimeout(() => {
				fetchItems(textInput.value);
			}, 250);
		});

		textInput.addEventListener("keydown", (event) => {
			if (!menu.classList.contains("show")) {
				if (event.key === "ArrowDown") {
					event.preventDefault();
					fetchItems(textInput.value);
				}
				return;
			}

			if (event.key === "ArrowDown") {
				event.preventDefault();
				highlightedIndex = Math.min(highlightedIndex + 1, items.length - 1);
				updateHighlight();
			} else if (event.key === "ArrowUp") {
				event.preventDefault();
				highlightedIndex = Math.max(highlightedIndex - 1, 0);
				updateHighlight();
			} else if (event.key === "Enter") {
				event.preventDefault();
				if (highlightedIndex >= 0) {
					selectItem(highlightedIndex);
				}
			} else if (event.key === "Escape") {
				event.preventDefault();
				closeMenu();
			}
		});

		textInput.addEventListener("blur", () => {
			setTimeout(() => {
				closeMenu();
			}, 120);
		});

		document.addEventListener("click", (event) => {
			if (!dropdown.contains(event.target)) {
				closeMenu();
			}
		});
	});
}

function initGlobalSearch() {
	const forms = document.querySelectorAll("[data-global-search='true']");

	forms.forEach((form) => {
		const input = form.querySelector("input[name='query']");
		const resultsPanel = form.querySelector(".hm-global-search__results");
		if (!input || !resultsPanel) {
			return;
		}

		let items = [];
		let highlightedIndex = -1;
		let debounceTimer;
		let activeController;

		const closeResults = () => {
			resultsPanel.classList.remove("show");
			resultsPanel.innerHTML = "";
			input.setAttribute("aria-expanded", "false");
			highlightedIndex = -1;
		};

		const openResults = () => {
			resultsPanel.classList.add("show");
			input.setAttribute("aria-expanded", "true");
		};

		const getItemUrl = (item) => item.url || item.Url || "#";
		const getItemTitle = (item) => item.title || item.Title || "";
		const getItemDescription = (item) => item.description || item.Description || "";
		const getItemBadge = (item) => item.badge || item.Badge || "";
		const getItemCategory = (item) => item.category || item.Category || "";

		const updateHighlight = () => {
			const links = resultsPanel.querySelectorAll(".hm-global-search__result");
			links.forEach((link, index) => {
				link.classList.toggle("is-active", index === highlightedIndex);
				link.setAttribute("aria-selected", index === highlightedIndex ? "true" : "false");
			});
		};

		const navigateToItem = (index) => {
			if (index < 0 || index >= items.length) {
				return;
			}

			window.location.href = getItemUrl(items[index]);
		};

		const createEmptyState = (message) => {
			const empty = document.createElement("div");
			empty.className = "hm-global-search__empty";
			empty.textContent = message;
			return empty;
		};

		const renderResults = () => {
			resultsPanel.innerHTML = "";

			if (items.length === 0) {
				resultsPanel.appendChild(createEmptyState("No matching pages or data found"));
				openResults();
				return;
			}

			const list = document.createElement("div");
			list.className = "hm-global-search__list";

			items.forEach((item, index) => {
				const link = document.createElement("a");
				link.className = "hm-global-search__result";
				link.href = getItemUrl(item);
				link.setAttribute("role", "option");
				link.setAttribute("aria-selected", "false");

				const badge = document.createElement("span");
				badge.className = "hm-global-search__badge";
				badge.textContent = getItemBadge(item) || getItemCategory(item);

				const body = document.createElement("span");
				body.className = "hm-global-search__body";

				const title = document.createElement("strong");
				title.textContent = getItemTitle(item);

				const description = document.createElement("small");
				description.textContent = getItemDescription(item);

				body.appendChild(title);
				body.appendChild(description);
				link.appendChild(badge);
				link.appendChild(body);

				link.addEventListener("mouseenter", () => {
					highlightedIndex = index;
					updateHighlight();
				});

				list.appendChild(link);
			});

			resultsPanel.appendChild(list);
			highlightedIndex = 0;
			updateHighlight();
			openResults();
		};

		const fetchResults = async () => {
			const term = input.value.trim();
			if (term.length < 2) {
				closeResults();
				return;
			}

			if (activeController) {
				activeController.abort();
			}

			activeController = new AbortController();

			try {
				const response = await fetch(`/search/suggestions?q=${encodeURIComponent(term)}`, {
					method: "GET",
					signal: activeController.signal,
					headers: {
						"X-Requested-With": "XMLHttpRequest"
					}
				});

				if (!response.ok) {
					return;
				}

				const data = await response.json();
				items = Array.isArray(data) ? data : [];
				renderResults();
			} catch {
				if (activeController.signal.aborted) {
					return;
				}

				resultsPanel.innerHTML = "";
				resultsPanel.appendChild(createEmptyState("Search is unavailable right now"));
				openResults();
			}
		};

		input.addEventListener("input", () => {
			clearTimeout(debounceTimer);
			debounceTimer = setTimeout(fetchResults, 220);
		});

		input.addEventListener("focus", () => {
			if (input.value.trim().length >= 2) {
				fetchResults();
			}
		});

		input.addEventListener("keydown", (event) => {
			if (!resultsPanel.classList.contains("show")) {
				return;
			}

			if (event.key === "ArrowDown") {
				event.preventDefault();
				highlightedIndex = Math.min(highlightedIndex + 1, items.length - 1);
				updateHighlight();
			} else if (event.key === "ArrowUp") {
				event.preventDefault();
				highlightedIndex = Math.max(highlightedIndex - 1, 0);
				updateHighlight();
			} else if (event.key === "Enter" && highlightedIndex >= 0) {
				event.preventDefault();
				navigateToItem(highlightedIndex);
			} else if (event.key === "Escape") {
				event.preventDefault();
				closeResults();
			}
		});

		document.addEventListener("click", (event) => {
			if (!form.contains(event.target)) {
				closeResults();
			}
		});
	});
}
